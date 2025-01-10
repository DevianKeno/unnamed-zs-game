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
        public string OwnerId;
        public int MaxPlayers;
        public int DifficultyLevel;
        public int DayLengthSeconds;
        public int NightLengthSeconds;
        public int Day;
        public int Hour;
        public int Minute;
        public System.Numerics.Vector3 WorldSpawn = new(0, 64, 0);
        
        public List<ObjectSaveData> Objects = new();
        public List<EntitySaveData> EntitySaves = new();
        public List<PlayerSaveData> PlayerSaves = new();
        public Dictionary<string, PlayerSaveData> PlayerIdSaves = new();
    }
}