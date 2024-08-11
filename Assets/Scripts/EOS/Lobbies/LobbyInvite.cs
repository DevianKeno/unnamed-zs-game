using Epic.OnlineServices;
using Epic.OnlineServices.Lobby;

namespace UZSG.EOS.Lobbies
{
    /// <summary>
    /// Class represents all Lobby Invite properties
    /// </summary>
    public class LobbyInvite
    {
        public Lobby Lobby = new Lobby();
        public LobbyDetails LobbyInfo = new LobbyDetails();
        public ProductUserId FriendId;
        public EpicAccountId FriendEpicId;
        public string FriendDisplayName;
        public string InviteId;

        public bool IsValid()
        {
            return Lobby.IsValid();
        }

        public void Clear()
        {
            Lobby.Clear();
            LobbyInfo.Release();
            FriendId = new ProductUserId();
            FriendEpicId = new EpicAccountId();
            FriendDisplayName = string.Empty;
            InviteId = string.Empty;
        }
    }
}