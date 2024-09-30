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
        public System.Numerics.Vector3 WorldSpawn = new(0, 64, 0);
        
        public List<ObjectSaveData> Objects = new();
        public List<EntitySaveData> EntitySaves = new();
        public List<PlayerSaveData> PlayerSaves = new();
        public Dictionary<string, PlayerSaveData> PlayerIdSaves = new();
    }
}