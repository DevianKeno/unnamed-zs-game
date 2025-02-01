using System;

namespace UZSG.Parties
{
    public struct PendingInvite : IEquatable<PendingInvite>
    {
        /// <summary>
        /// The client Id of who's being invited.
        /// </summary>
        public ulong ClientId { get; set; }
        public float SentTime { get; set;}

        public bool Equals(PendingInvite other)
        {
            return ClientId == other.ClientId;
        }

        public PendingInvite(ulong clientId, float sentTime)
        {
            ClientId = clientId;
            SentTime = sentTime;
        }
    }
}