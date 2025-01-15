using System;

namespace UZSG.EOS.P2P
{
    [Serializable]
    public class Packet
    {
        public PacketType Type;
        public byte[] Data;
    }
}