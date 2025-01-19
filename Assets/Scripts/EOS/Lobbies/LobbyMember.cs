using System.Collections.Generic;

using Epic.OnlineServices;

namespace UZSG.EOS.Lobbies
{
    /// <summary>
    /// Class represents all Lobby Member properties
    /// </summary>
    public class LobbyMember
    {
        public LobbyMember(ProductUserId productUserId)
        {
            this.ProductId = productUserId;
        }

        // public EpicAccountId AccountId;
        public ProductUserId ProductId { get; private set; }

        public string DisplayName
        {
            get
            {
                MemberAttributes.TryGetValue(DisplayNameKey, out LobbyAttribute nameAttrib);
                return nameAttrib?.AsString ?? string.Empty;
            }
        }

        public const string DisplayNameKey = "DISPLAYNAME";

        public Dictionary<string, LobbyAttribute> MemberAttributes = new Dictionary<string, LobbyAttribute>();

        public LobbyRTCState RTCState = new LobbyRTCState();
    }
}