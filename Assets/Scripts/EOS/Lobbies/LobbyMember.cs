using System.Collections.Generic;

using Epic.OnlineServices;

namespace UZSG.EOS.Lobbies
{
    /// <summary>
    /// Class represents all Lobby Member properties
    /// </summary>
    public class LobbyMember
    {
        public ProductUserId ProductUserId { get; private set; }
        public string DisplayName { get; set; } = string.Empty;
        Dictionary<string, LobbyAttribute> _attributes = new();
        public LobbyRTCState RTCState = new();
        
        public LobbyMember(ProductUserId productUserId)
        {
            this.ProductUserId = productUserId;
        }

        public void AddAttribute(LobbyAttribute attribute)
        {
            if (attribute == null) return;
            
            this._attributes[attribute.Key] = attribute;
        }

        public bool TryGetAttribute(string key, out LobbyAttribute attribute)
        {
            return this._attributes.TryGetValue(key, out attribute);
        }
    }
}