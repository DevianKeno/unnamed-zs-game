namespace UZSG.EOS.P2P
{
    /// <summary>
    /// Stores cached chat data in <c>UIPeer2PeerMenu</c>.
    /// </summary>
    public struct ChatEntry
    {
        /// <value>True if message was from local user</value>
        public bool IsOwnEntry { get; set; }
        /// <value> Cache for message entry </value>
        public string Message { get; set; }
    }
}