using UnityEngine;

using PlayEveryWare.EpicOnlineServices;

using UZSG.Systems;

namespace UZSG.EOS
{
    public class P2PHandler : MonoBehaviour, IAuthInterfaceEventListener, IConnectInterfaceEventListener
    {
        [SerializeField] int refreshRate = 60;
        float _timer;

        void Start()
        {
            Game.EOS.AddAuthLoginListener(this);
            Game.EOS.AddAuthLogoutListener(this);
            Game.EOS.AddConnectLoginListener(this);
        }

        void Update()
        {
            _timer += Time.deltaTime;
            if (_timer >= 1 / refreshRate)
            {
                _timer = 0f;
                EOSSubManagers.P2P.Update();
            }
        }

        public void OnAuthLogin(Epic.OnlineServices.Auth.LoginCallbackInfo info)
        {
            /// NOTE: Idk really know which is used when logging if its either Auth or Connect
            /// but I'm team connect for now
            
            // if (info.ResultCode == Result.Success)
            // {
            //      Game.Main.AddUpdateCoListener((Game.IUpdateCoListener) this);
            // }
        }

        public void OnAuthLogout(Epic.OnlineServices.Auth.LogoutCallbackInfo info)
        {
            if (info.ResultCode == Epic.OnlineServices.Result.Success)
            {
                
            }
        }

        public void OnConnectLogin(Epic.OnlineServices.Connect.LoginCallbackInfo info)
        {
            if (info.ResultCode == Epic.OnlineServices.Result.Success)
            {
                
            }
        }
    }
}