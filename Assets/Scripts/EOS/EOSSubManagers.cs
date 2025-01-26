using UnityEngine;

using UZSG.Systems;

namespace UZSG.EOS
{
    /// <summary>
    /// Wrapper for EOS sub managers.
    /// </summary>
    public class EOSSubManagers : MonoBehaviour
    {
        // public DiscordManager Discord => DiscordManager.Instance;
        public static EOSAuthManager Auth => Game.EOS.GetOrCreateManager<EOSAuthManager>();
        public static EOSUserInfoManager UserInfo => Game.EOS.GetOrCreateManager<EOSUserInfoManager>();
        public static EOSFriendsManager Friends => Game.EOS.GetOrCreateManager<EOSFriendsManager>();
        public static EOSLobbyManager Lobbies => Game.EOS.GetOrCreateManager<EOSLobbyManager>();
        public static EOSPeer2PeerManager P2P => Game.EOS.GetOrCreateManager<EOSPeer2PeerManager>();
        public static EOSTransportManager Transport => Game.EOS.GetOrCreateManager<EOSTransportManager>();

        public static void Initialize()
        {
            Game.EOS.GetOrCreateManager<EOSAuthManager>();
            Game.EOS.GetOrCreateManager<EOSTransportManager>();
            Game.EOS.GetOrCreateManager<EOSUserInfoManager>();
            Game.EOS.GetOrCreateManager<EOSFriendsManager>();
            Game.EOS.GetOrCreateManager<EOSLobbyManager>();
            Game.EOS.GetOrCreateManager<EOSPeer2PeerManager>();
        }
    }
}