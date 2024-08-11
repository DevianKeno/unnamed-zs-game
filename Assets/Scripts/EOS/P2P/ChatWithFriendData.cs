using System.Collections.Generic;

using Epic.OnlineServices;

namespace UZSG.EOS.P2P
{
    /// <summary>
    /// Stores cached friend chat data in <c>UIPeer2PeerMenu</c>.
    /// </summary>
    public struct ChatWithFriendData
    {
        /// <value> Queue of cached <c>ChatEntry</c> objects </value>
        public Queue<ChatEntry> ChatLines { get; set; }
        /// <value> <c>FriendId</c> of remote friend </value>
        public ProductUserId FriendId { get; set; }
    }
}