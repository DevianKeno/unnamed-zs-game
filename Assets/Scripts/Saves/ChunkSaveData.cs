using System;
using System.Collections.Generic;

namespace UZSG.Saves
{
    [Serializable]
    public class ChunkSaveData : SaveData
    {
        public int[] Coord = new int[] { 0, 0, 0 };
        public List<BaseObjectSaveData> Objects = new();
    }
}
