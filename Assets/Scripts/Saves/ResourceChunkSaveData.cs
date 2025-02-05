using System;
using System.Collections.Generic;

namespace UZSG.Saves
{
    [Serializable]
    public class ResourceChunkSaveData : SaveData
    {
        int[] Coord = new int[] { 0, 0, 0 };
        List<ObjectSaveData> objects = new();
    }
}
