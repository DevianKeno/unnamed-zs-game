using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
using TMPro;

using Epic.OnlineServices;
using Epic.OnlineServices.Auth;
using Epic.OnlineServices.Connect;
using Epic.OnlineServices.UserInfo;
using PlayEveryWare.EpicOnlineServices;

namespace UZSG.EOS
{
    public class AuthUserDisplayUI : MonoBehaviour, IAuthInterfaceEventListener, IConnectInterfaceEventListener
    {
        [SerializeField] bool signInOnStart = false;

        [Header("Elements")]
        [SerializeField] Button signInBtn;
        [SerializeField] Button signOutBtn;
        [SerializeField] TextMeshProUGUI usernameTMP;
        // [SerializeField] LoadingIconAnimation loadingIcon;
        
        void Awake()
        {
            signInBtn.onClick.AddListener(StartLogin);
            signOutBtn.onClick.AddListener(StartLogout);
        }

        void OnDestroy()
        {
            signInBtn.onClick.RemoveListener(StartLogin);
            signOutBtn.onClick.RemoveListener(StartLogout);
            Game.EOS.RemoveAuthLoginListener(this);
            Game.EOS.RemoveConnectLoginListener(this);
        }

        void Start()
        {
            if (EOSSubManagers.Auth.GetLoginStatus() == LoginStatus.LoggedIn)
            {
                SetDisplayedAccount(EOSSubManagers.UserInfo.GetLocalUserInfo());
                SetUIForLogout();
                return;
            }
            
            Game.EOS.AddAuthLoginListener(this);
            Game.EOS.AddConnectLoginListener(this);
#if UNITY_EDITOR
            if (signInOnStart) 
#endif
                if (EOSSubManagers.Auth.RememberLogin)
                {
                    signInBtn.interactable = false;
                    usernameTMP.text = "Signing in...";
                    SetUIForLoading();
                    EOSSubManagers.Auth.StartPersistentLogin();
                    return;
                }

            SetUIForLogin();
        }


        #region Login flow
        
        void StartLogin()
        {
            signInBtn.interactable = false;
            usernameTMP.text = "Signing in...";
            SetUIForLoading();
            EOSSubManagers.Auth.StartLogin();
        }

        #endregion


        #region Logout flow
        public void StartLogout()
        {
            SetUIForLoading();
            usernameTMP.text = "Logging out...";

            var options = new Epic.OnlineServices.Connect.LogoutOptions()
            {
                LocalUserId = Game.EOS.GetProductUserId()
            };
            Game.EOS.GetEOSConnectInterface().Logout(ref options, null, OnLogoutCompleted);
        }

        void OnLogoutCompleted(ref Epic.OnlineServices.Connect.LogoutCallbackInfo info)
        {
        }
        
        #endregion


        #region EOS callbacks
        
        public void OnAuthLogin(Epic.OnlineServices.Auth.LoginCallbackInfo info)
        {
            // if (info.ResultCode == Epic.OnlineServices.Result.Success)
            // {
            //     Game.Console.LogInfo($"Fetching auth user info...");
            //     EOSSubManagers.UserInfo.QueryUserInfoByEpicId(info.LocalUserId, OnQueryUserInfoByEpicIdCompleted);
            //     SetUIForLogout();
            //     return;
            // }
            // else
            // {
            //     string msg = $"Encountered an error upon logging in: [{info.ResultCode}]";
            //     Game.Console.LogInfo(msg);
            //     Debug.Log(msg);
            //     SetUIForLogin();
            //     return;
            // }
        }

        void OnQueryUserInfoByEpicIdCompleted(UserInfoData userInfo, EpicAccountId accountId, Epic.OnlineServices.Result result)
        {
            if (result == Epic.OnlineServices.Result.Success)
            {
                SetDisplayedAccount(userInfo);
                return;
            }
            else
            {
                string msg = $"An error occured when retrieving user information: [{result}]";
                Game.Console.LogInfo(msg);
                Debug.Log(msg);
                usernameTMP.text = "Logged in user?";
                return;
            }
        }

        public void OnAuthLogout(Epic.OnlineServices.Auth.LogoutCallbackInfo info)
        {
        }

        public void OnConnectLogin(Epic.OnlineServices.Connect.LoginCallbackInfo info)
        {
            if (info.ResultCode == Epic.OnlineServices.Result.Success)
            {
                Game.Console.LogInfo($"Fetching connect user info...");
                EOSSubManagers.UserInfo.QueryUserInfoByProductId(info.LocalUserId, OnQueryUserInfoByProductIdCallback);
                SetUIForLogout();
                return;
            }
            else
            {
                string msg = $"Encountered an error upon logging in: [{info.ResultCode}]";
                Game.Console.LogInfo(msg);
                Debug.Log(msg);
                SetUIForLogin();
                return;
            }
        }

        void OnQueryUserInfoByProductIdCallback(ExternalAccountInfo userInfo, ProductUserId userId, Epic.OnlineServices.Result result)
        {
            if (result == Epic.OnlineServices.Result.Success)
            {
                SetDisplayedAccount(userInfo);
                return;
            }
            else
            {
                string msg = $"An error occured when retrieving user information: [{result}]";
                Game.Console.LogInfo(msg);
                Debug.LogError(msg);
                usernameTMP.text = "Logged in user ?";
            }
        }
        
        #endregion


        #region UI display methods

        void SetUIForLogin()
        {
            usernameTMP.text = "Not logged in";
            signInBtn.gameObject.SetActive(true);

            signOutBtn.gameObject.SetActive(false);
            // loadingIcon.gameObject.SetActive(false); 
        }
        
        void SetUIForLoading()
        {
            // loadingIcon.gameObject.SetActive(true);

            signInBtn.gameObject.SetActive(false);
            signOutBtn.gameObject.SetActive(false);
        }

        void SetUIForLogout()
        {
            /// DISABLED: technically disallowed to log out lol
            // signOutBtn.gameObject.SetActive(true);

            signInBtn.gameObject.SetActive(false);
            // loadingIcon.gameObject.SetActive(false); 
        }

        public void SetUIFakeLogin()
        {
            usernameTMP.text = "Signing in...";
            SetUIForLoading();
        }

        public void SetDisplayedAccount(UserInfoData info)
        {
            usernameTMP.text = info.DisplayName;
        }

        public void SetDisplayedAccount(ExternalAccountInfo info)
        {
            usernameTMP.text = info.DisplayName;
        }

        #endregion

    }
}