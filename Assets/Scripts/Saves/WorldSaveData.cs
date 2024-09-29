using System;
using System.Collections.Generic;

namespace UZSG.Saves
{
    [Serializable]
    public class WorldSaveData : SaveData
    {
        public string Name;
        public string LevelId;
        public DateTime CreatedDate;
        public DateTime LastModifiedDate;
        public DateTime LastPlayedDate;
        
        public List<ObjectSaveData> Objects = new();
        public List<EntitySaveData> EntitySaves = new();
        public List<PlayerSaveData> PlayerSaves = new();
        public Dictionary<string, PlayerSaveData> PlayerIdSaves = new();
    }
}