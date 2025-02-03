using System;
using System.Collections.Generic;

namespace UZSG.Saves
{
    [Serializable]
    public class EnemySaveData : EntitySaveData
    {
        public static readonly EnemySaveData Empty = new();

        public bool IsNaturallySpawned = false;
    }
}
