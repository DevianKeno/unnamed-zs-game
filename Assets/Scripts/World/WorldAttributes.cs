using System;

namespace UZSG.World
{
    [Serializable]
    public class WorldAttributes
    {
        public int ChunkSize = 16;
        public int WorldHeight = 256;
        public int SeaLevel = 64;        
        public int renderDistance = 12;

        public int ChunkCount
        {
            get => (ChunkSize + 3) * ( WorldHeight + 1) * (WorldHeight + 3);
        }
    }
}