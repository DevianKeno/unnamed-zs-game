using System;

namespace UZSG.EOS.P2P
{
    [Serializable]
    public struct Packet
    {
        public byte Type;
        public byte[] Data;

        public readonly byte[] ToBytes()
        {
            byte[] result = new byte[1 + (Data?.Length ?? 0)];
            result[0] = Type;
            if (Data != null)
            {
                Array.Copy(Data, 0, result, 1, Data.Length);
            }
            return result;
        }
    }
}