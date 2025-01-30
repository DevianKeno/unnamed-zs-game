/*
* Copyright (c) 2021 PlayEveryWare
* 
* Permission is hereby granted, free of charge, to any person obtaining a copy
* of this software and associated documentation files (the "Software"), to deal
* in the Software without restriction, including without limitation the rights
* to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
* copies of the Software, and to permit persons to whom the Software is
* furnished to do so, subject to the following conditions:
* 
* The above copyright notice and this permission notice shall be included in all
* copies or substantial portions of the Software.
* 
* THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
* OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
* SOFTWARE.
*/

using System;
using System.Collections.Generic;

using UnityEngine;

using Epic.OnlineServices;
using Epic.OnlineServices.UserInfo;
using Epic.OnlineServices.Auth;
using Epic.OnlineServices.Connect;
using PlayEveryWare.EpicOnlineServices;

using UZSG.Systems;

namespace UZSG.EOS
{
    /// <summary>
    /// General purpose access point for user info, including local user.
    /// </summary>
    public class EOSUserInfoManager : IEOSSubManager, IAuthInterfaceEventListener, IConnectInterfaceEventListener
    {
        public string LocalUserDisplayName { get; set; }
        UserInfoInterface userInfoInterface;
        UserInfoData localUserInfo;
        ExternalAccountInfo externalAccountInfo;

        Dictionary<EpicAccountId, UserInfoData> epicIdUserInfoMappings = new();
        Dictionary<ProductUserId, ExternalAccountInfo> productIdUserInfoMappings = new();
        /// <summary>
        /// <c>string</c> is DisplayName.
        /// </summary>
        Dictionary<string, UserInfoData> displayNameUserInfoMappings = new();
        Dictionary<string, ExternalAccountInfo> displayNameExternalMappings = new();
        // List<OnUserInfoChangedCallback> localUserInfoChangedCallbacks = new();

        public delegate void OnUserInfoChangedCallback(UserInfoData newUserInfo);
        public delegate void QueryUserInfoByEpicIdCallback(UserInfoData userInfo, EpicAccountId accountId, Epic.OnlineServices.Result result);
        public delegate void QueryUserInfoProductIdCallback(ExternalAccountInfo userInfo, ProductUserId userId, Epic.OnlineServices.Result result);
        public delegate void QueryUserInfoDisplayNameCallback(string displayName, Epic.OnlineServices.Result result);

        public EOSUserInfoManager()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.playModeStateChanged -= OnPlayModeChanged;
            UnityEditor.EditorApplication.playModeStateChanged += OnPlayModeChanged;
#endif
            Game.EOS.AddAuthLoginListener(this);
            Game.EOS.AddConnectLoginListener(this);
            userInfoInterface = Game.EOS.GetEOSUserInfoInterface();
        }

#if UNITY_EDITOR
        ~EOSUserInfoManager()
        {
            UnityEditor.EditorApplication.playModeStateChanged -= OnPlayModeChanged;
        }
#endif

#if UNITY_EDITOR
        void OnPlayModeChanged(UnityEditor.PlayModeStateChange modeChange)
        {
            if (modeChange == UnityEditor.PlayModeStateChange.ExitingPlayMode)
            {
                /// Prevent attempts to call native EOS code while exiting play mode, which crashes the editor
                userInfoInterface = null;
            }
        }
#endif

        #region EOS callbacks

        public void OnAuthLogin(Epic.OnlineServices.Auth.LoginCallbackInfo info)
        {
            QueryUserInfoByEpicId(info.LocalUserId);
        }
        
        public void OnAuthLogout(Epic.OnlineServices.Auth.LogoutCallbackInfo info)
        {
        }

        public void OnConnectLogin(Epic.OnlineServices.Connect.LoginCallbackInfo info)
        {
            QueryUserInfoByProductId(info.LocalUserId);
        }

        #endregion


        public UserInfoData GetLocalUserInfo()
        {
            return localUserInfo;
        }

        public void ClearUserInfo()
        {
            epicIdUserInfoMappings.Clear();
            displayNameUserInfoMappings.Clear();
            
            if (localUserInfo.UserId.IsValid())
            {
                /// At minimum, local user info should be cached
                epicIdUserInfoMappings.Add(localUserInfo.UserId, localUserInfo);
                if (!string.IsNullOrEmpty(localUserInfo.DisplayName))
                {
                    displayNameUserInfoMappings.Add(localUserInfo.DisplayName, localUserInfo);
                }
            }
        }

        public UserInfoData GetUserInfoByEpicId(EpicAccountId UserId)
        {
            epicIdUserInfoMappings.TryGetValue(UserId, out UserInfoData userInfo);
            return userInfo;
        }

        public bool TryGetUserInfoByProductId(ProductUserId userId, out ExternalAccountInfo userInfo)
        {
            return productIdUserInfoMappings.TryGetValue(userId, out userInfo);
        }

        public UserInfoData GetUserInfoByDisplayName(string DisplayName)
        {
            displayNameUserInfoMappings.TryGetValue(DisplayName, out UserInfoData userInfo);
            return userInfo;
        }
        
        public struct QueryUserInfoResult
        {
            public Epic.OnlineServices.Result Result { get; set; }
            public UserInfoData UserInfoData { get; set; }
        }


        #region EpicAccountId

        public void QueryUserInfoByEpicId(EpicAccountId epicId, QueryUserInfoByEpicIdCallback callback = null)
        {
            if (epicId == null || !epicId.IsValid())
            {
                Debug.LogError("[UserInfo/QueryUserInfoByEpicId()]: Invalid EpicAccountId");
                callback?.Invoke(default, epicId, Epic.OnlineServices.Result.InvalidUser);
                return;
            }

            if (epicIdUserInfoMappings.TryGetValue(epicId, out var userInfo))
            {
                callback?.Invoke(userInfo, epicId, Epic.OnlineServices.Result.Success);
                return;
            }

            var options = new QueryUserInfoOptions()
            {
                LocalUserId = Game.EOS.GetLocalUserId(),
                TargetUserId = epicId
            };
            userInfoInterface.QueryUserInfo(ref options, callback, OnQueryUserInfoCompleted);
        }

        void OnQueryUserInfoCompleted(ref QueryUserInfoCallbackInfo info)
        {
            if (info.ResultCode == Epic.OnlineServices.Result.Success)
            {
                Debug.Log("[UserInfoManager/OnQueryUserInfoCompleted()]: QueryUserInfo successful");

                var options = new CopyUserInfoOptions()
                {
                    LocalUserId = info.LocalUserId,
                    TargetUserId = info.TargetUserId
                };
                Epic.OnlineServices.Result result = userInfoInterface.CopyUserInfo(ref options, out UserInfoData? userInfo);
                if (result == Epic.OnlineServices.Result.Success && userInfo.HasValue)
                {
                    CacheUserInfo(userInfo.Value);
                    if (info.ClientData is QueryUserInfoByEpicIdCallback callback)
                    {
                        callback?.Invoke(userInfo.Value, info.TargetUserId, info.ResultCode);
                    }
                    return;
                }
            }

            Debug.LogError($"[UserInfoManager/OnQueryUserInfoCompleted()]: An error occured error: [{info.ResultCode}]");
            if (info.ClientData is QueryUserInfoByEpicIdCallback callback1)
            {
                callback1?.Invoke(default, info.TargetUserId, info.ResultCode);
            }
        }
        
        #endregion


        #region ProductUserId

        public void QueryUserInfoByProductId(ProductUserId userId, QueryUserInfoProductIdCallback onCompleted = null)
        {
            if (userId == null || !userId.IsValid())
            {
                Debug.LogError("[UserInfoManager/QueryUserInfoByProductId()]: Invalid ProductUserId");
                onCompleted?.Invoke(default, null, Epic.OnlineServices.Result.InvalidUser);
                return;
            }

            if (productIdUserInfoMappings.TryGetValue(userId, out var userInfo))
            {
                onCompleted?.Invoke(userInfo, userId, Epic.OnlineServices.Result.Success);
                return;
            }

            var options = new QueryProductUserIdMappingsOptions()
            {
                LocalUserId = Game.EOS.GetProductUserId(),
                ProductUserIds = new ProductUserId[] { userId }
            };
            Game.EOS.GetEOSConnectInterface().QueryProductUserIdMappings(ref options, onCompleted, QueryProductUserIdMappingsCompleted);
        }

        void QueryProductUserIdMappingsCompleted(ref QueryProductUserIdMappingsCallbackInfo info)
        {
            if (info.ResultCode == Epic.OnlineServices.Result.Success)
            {
                var options = new CopyProductUserInfoOptions()
                {
                    TargetUserId = info.LocalUserId
                };
                var result = Game.EOS.GetEOSConnectInterface().CopyProductUserInfo(ref options, out ExternalAccountInfo? outUserInfo);
                if (result == Epic.OnlineServices.Result.Success && outUserInfo.HasValue)
                {
                    CacheUserInfo(outUserInfo.Value);
                    if (info.ClientData is QueryUserInfoProductIdCallback callback)
                    {
                        callback?.Invoke(outUserInfo.Value, info.LocalUserId, info.ResultCode);
                    }
                    return;
                }
            }

            if (info.ClientData is QueryUserInfoProductIdCallback callback1)
            {
                callback1?.Invoke(default, info.LocalUserId, info.ResultCode);
            }
        }

        #endregion


        #region DisplayName

        public void QueryUserInfoByDisplayName(string DisplayName, QueryUserInfoDisplayNameCallback Callback = null)
        {
            if (string.IsNullOrEmpty(DisplayName))
            {
                Debug.LogError("UserInfo (QueryUserInfoByDisplayName): Invalid Display Name");
                Callback?.Invoke(DisplayName, Epic.OnlineServices.Result.InvalidParameters);
                return;
            }

            QueryUserInfoByDisplayNameOptions options = new QueryUserInfoByDisplayNameOptions()
            {
                LocalUserId = Game.EOS.GetLocalUserId(),
                DisplayName = DisplayName
            };

            userInfoInterface.QueryUserInfoByDisplayName(ref options, Callback, OnQueryUserInfoDisplayNameCompleted);
        }

        void OnQueryUserInfoDisplayNameCompleted(ref QueryUserInfoByDisplayNameCallbackInfo data)
        {
            QueryUserInfoDisplayNameCallback callback = data.ClientData as QueryUserInfoDisplayNameCallback;

            //QueryUserInfoByDisplayNameCallbackInfo.DisplayName memory is only valid within callback scope, so copy for safety
            string displayNameCopy = string.Copy(data.DisplayName);

            if (data.ResultCode != Epic.OnlineServices.Result.Success)
            {
                Debug.LogErrorFormat("UserData (OnQueryUserInfoDisplayNameCompleted): Error calling QueryUserInfoByDisplayName: {0}", data.ResultCode);
                callback?.Invoke(displayNameCopy, data.ResultCode);
                return;
            }

            Debug.Log("UserData (OnQueryUserInfoDisplayNameCompleted): QueryUserInfoByDisplayName successful");

            var options = new CopyUserInfoOptions()
            {
                LocalUserId = data.LocalUserId,
                TargetUserId = data.TargetUserId
            };
            Epic.OnlineServices.Result result = userInfoInterface.CopyUserInfo(ref options, out UserInfoData? userInfo);

            if (result != Epic.OnlineServices.Result.Success)
            {
                Debug.LogErrorFormat("UserData (OnQueryUserInfoDisplayNameCompleted): CopyUserInfo error: {0}", result);
                callback?.Invoke(displayNameCopy, result);
                return;
            }

            if (userInfo != null)
            {
                CacheUserInfo((UserInfoData) userInfo);
            }

            callback?.Invoke(displayNameCopy, data.ResultCode);
        }

        #endregion

        public void RemoveCachedInfo(EpicAccountId epicId)
        {
            if (epicIdUserInfoMappings.ContainsKey(epicId))
            {
                epicIdUserInfoMappings.Remove(epicId);
            }
        }

        public void RemoveCachedInfo(ProductUserId userId)
        {
            if (productIdUserInfoMappings.ContainsKey(userId))
            {
                productIdUserInfoMappings.Remove(userId);
            }
        }

        void CacheUserInfo(UserInfoData userInfo)
        {
            epicIdUserInfoMappings[userInfo.UserId] = userInfo;
            if (!string.IsNullOrEmpty(userInfo.DisplayName))
            {
                displayNameUserInfoMappings[userInfo.DisplayName] = userInfo;
            }

            if (userInfo.UserId == Game.EOS.GetLocalUserId())
            {
                localUserInfo = userInfo;
                LocalUserDisplayName = userInfo.DisplayName;
                // foreach (var callback in localUserInfoChangedCallbacks)
                // {
                //     callback?.Invoke(localUserInfo);
                // }
            }
        }

        void CacheUserInfo(ExternalAccountInfo userInfo)
        {
            productIdUserInfoMappings[userInfo.ProductUserId] = userInfo;
            if (!string.IsNullOrEmpty(userInfo.DisplayName))
            {
                displayNameExternalMappings[userInfo.DisplayName] = userInfo;
            }

            if (userInfo.ProductUserId == Game.EOS.GetProductUserId())
            {
                externalAccountInfo = userInfo;
                LocalUserDisplayName = userInfo.DisplayName;
                // foreach (var callback in localUserInfoChangedCallbacks)
                // {
                //     callback?.Invoke(localUserInfo);
                // }
            }
        }

        // public void AddNotifyLocalUserInfoChanged(OnUserInfoChangedCallback Callback)
        // {
        //     localUserInfoChangedCallbacks.Add(Callback);
        // }

        // public void RemoveNotifyLocalUserInfoChanged(OnUserInfoChangedCallback Callback)
        // {
        //     localUserInfoChangedCallbacks.Remove(Callback);
        // }
    }
}