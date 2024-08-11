namespace UZSG.EOS.P2P
{
    public struct MessageData
    {
        public MessageType Type { get; set; }
        public string TextData { get; set; }
        public float PositionX { get; set; }
        public float PositionY { get; set; }
    }
}