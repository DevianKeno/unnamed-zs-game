using System;

namespace UZSG.EOS.P2P
{
    [Serializable]
    public struct Packet
    {
        public PacketType Type { get; set; }
        public PacketSubtype Subtype { get; set; }
        public ArraySegment<byte> Data { get; set; }
        
        public readonly byte[] ToBytes()
        {
            byte[] result = new byte[2 + Data.Count];
            result[0] = (byte) Type;
            result[1] = (byte) Subtype;

            if (Data != null && Data.Count > 0)
            {
                Array.Copy(Data.Array, Data.Offset, result, 2, Data.Count);
            }

            return result;
        }

        public static Packet FromBytes(byte[] bytes)
        {
            if (bytes == null || bytes.Length < 2)
            {
                throw new ArgumentException("Invalid byte array. It must contain at least two bytes for the type and subtype.");
            }

            return new Packet
            {
                Type = (PacketType) bytes[0],           // First byte is the Type
                Subtype = (PacketSubtype) bytes[1],     // Second byte is the Subtype
                Data = bytes.Length > 2                 // Remaining bytes are the Data payload
                    ? new ArraySegment<byte>(bytes, 2, bytes.Length - 2)
                    : ArraySegment<byte>.Empty
            };
        }
    }
}