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
        public static EOSUserInfoManager UserInfo => Game.EOS.GetOrCreateManager<EOSUserInfoManager>();
        public static EOSFriendsManager Friends => Game.EOS.GetOrCreateManager<EOSFriendsManager>();
        public static EOSLobbyManager Lobbies => Game.EOS.GetOrCreateManager<EOSLobbyManager>();
        public static EOSPeer2PeerManager P2P => Game.EOS.GetOrCreateManager<EOSPeer2PeerManager>();
    }
}