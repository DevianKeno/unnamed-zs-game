using System;
using System.Collections.Generic;

namespace UZSG.Saves
{

    [Serializable]
    public class WorldSaveData : SaveData
    {
        public List<ObjectSaveData> Objects;
        public List<EntitySaveData> EntitySaves;
        public List<PlayerSaveData> PlayerSaves;
    }
}