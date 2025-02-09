using System;
using System.Collections.Generic;

using UnityEngine;

using Epic.OnlineServices;
using Epic.OnlineServices.Auth;
using Epic.OnlineServices.Connect;
using PlayEveryWare.EpicOnlineServices;

namespace UZSG.EOS
{
    public class EOSAuthManager : IEOSSubManager
    {
        AuthInterface authInterface = Game.EOS.GetEOSAuthInterface();
        public LoginCredentialType LoginType = LoginCredentialType.PersistentAuth;
        public bool RememberLogin = false;

        bool isLoggedIn = false;
        public bool IsLoggedIn => isLoggedIn;


        #region Login flow

        public void StartPersistentLogin()
        {
            Game.Console.LogInfo($"Logging in via persistent auth...");
            LoginType = LoginCredentialType.PersistentAuth;
            StartLogin();
        }

        public void StartDevAuthLogin(string username, string password)
        {
            Game.Console.LogInfo($"Logging in via developer portal...");
            Game.EOS.StartLoginWithLoginTypeAndToken(LoginCredentialType.Developer, username, password, OnAuthLoginCallback);
        }

        public void StartLogin()
        {
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                Debug.Log($"Internet not reachable.");
                return;
            }
         
            if (LoginType == LoginCredentialType.PersistentAuth)
            {
                Game.EOS.StartPersistentLogin(OnAuthLoginCallback);
            }
            else if (LoginType == LoginCredentialType.AccountPortal)
            {
                Game.Console.LogInfo($"Logging in via account portal...");
                Game.EOS.StartLoginWithLoginTypeAndToken(
                    LoginCredentialType.AccountPortal,
                    null,
                    null,
                    OnAuthLoginCallback);
            }
            else if (LoginType == LoginCredentialType.ExternalAuth)
            {
                Game.Console.LogInfo($"Logging in via external auth...");
                // ConnectWithDiscord();
            }
            else
            {
                Game.Console.LogInfo("Unhandled login type." + LoginType.ToString());
                Debug.LogError("Unhandled login type." + LoginType.ToString());
            }
        }

        void OnAuthLoginCallback(Epic.OnlineServices.Auth.LoginCallbackInfo info)
        {
            if (info.ResultCode == Epic.OnlineServices.Result.Success)
            {
                Game.EOS.StartConnectLoginWithEpicAccount(info.LocalUserId, OnConnectLoginCallback);
                return;
            }
            else
            {
                string msg = $"Encountered an error when logging in: [{info.ResultCode}]";
                Game.Console.LogInfo(msg);
                Debug.LogError(msg);

                if (LoginType == LoginCredentialType.PersistentAuth)
                {
                    LoginType = LoginCredentialType.AccountPortal;
                    Game.EOS.StartLoginWithLoginTypeAndToken(
                        LoginCredentialType.AccountPortal,
                        null,
                        null,
                        OnAuthLoginCallback);
                }
            }
        }

        void OnConnectLoginCallback(Epic.OnlineServices.Connect.LoginCallbackInfo info)
        {
            if (info.ResultCode == Epic.OnlineServices.Result.Success)
            {
                Game.Console.LogInfo($"Logged in successfully");
                isLoggedIn = true;
                EOSSubManagers.Initialize();
                return;
            }
            else
            {
                /// Create new Connect user
                Game.EOS.CreateConnectUserWithContinuanceToken(info.ContinuanceToken, (CreateUserCallbackInfo createUserCallbackInfo) =>
                {
                    if (createUserCallbackInfo.ResultCode == Epic.OnlineServices.Result.Success)
                    {
                        Game.Console.LogInfo($"Logged in successfully");
                        isLoggedIn = true;
                        EOSSubManagers.Initialize();
                        return;
                    }
                    else
                    {
                        string msg = $"Encountered an error when logging in: [{info.ResultCode}]";
                        Game.Console.LogInfo(msg);
                        Debug.LogError(msg);
                    }
                });
            }
        }

        #endregion


        #region Logout flow

        public void StartLogOut()
        {
            var options = new Epic.OnlineServices.Connect.LogoutOptions()
            {
                LocalUserId = Game.EOS.GetProductUserId()
            };
            Game.EOS.GetEOSConnectInterface().Logout(ref options, null, OnLogoutCompleted);
        }

        void OnLogoutCompleted(ref Epic.OnlineServices.Connect.LogoutCallbackInfo info)
        {
            if (info.ResultCode == Epic.OnlineServices.Result.Success)
            {
                var options = new UnlinkAccountOptions()
                {
                    LocalUserId = Game.EOS.GetProductUserId()
                };
                Game.EOS.GetEOSConnectInterface().UnlinkAccount(ref options, null, OnUnlinkAccountCompleted);
                return;
            }

            Game.Console.LogInfo($"Encountered an error upon logging out. [{info.ResultCode}]");
            Debug.LogWarning($"Encountered an error upon logging out. [{info.ResultCode}]");
        }

        void OnUnlinkAccountCompleted(ref UnlinkAccountCallbackInfo data)
        {
            if (data.ResultCode == Epic.OnlineServices.Result.Success)
            {
                return;
            }
            
            Game.Console.LogInfo($"Unlink error. [{data.ResultCode}]");
            Debug.LogWarning($"Unlink error. [{data.ResultCode}]");
        }

        #endregion


        public void RemovePersistentToken()
        {
            Game.EOS.RemovePersistentToken();
        }
        
        public LoginStatus GetLoginStatus()
        {
            var localUserId = Game.EOS.GetLocalUserId();
            if (localUserId != null && localUserId.IsValid())
            {
                return authInterface.GetLoginStatus(localUserId);
            }

            return LoginStatus.NotLoggedIn;
        }
    }
}