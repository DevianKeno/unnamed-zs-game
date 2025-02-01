using Epic.OnlineServices;

using UZSG.Entities;

namespace UZSG.Parties
{
    public class PartyMember
    {
        public ulong ClientId { get; set; }
        public string DisplayName { get; set; }
        public ProductUserId ProductUserId { get; set; }
        /// <summary>
        /// Reference to the Player entity.
        /// </summary>
        public Player Player { get; set; }
    }
}