using System.Collections.Generic;
using System;

using UnityEngine;
using MessagePack;

using UZSG.Saves;

namespace UZSG.Worlds
{
    [Serializable]
    [MessagePackObject]
    public class WorldSaveData : SaveData
    {
        [Key(0)]
        public List<UserObjectSaveData> Objects;
        [Key(1)]
        public List<EntitySaveData> EntitySaves;
        [Key(2)]
        public List<PlayerSaveData> PlayerSaves;
    }

    [MessagePackObject]
    public class UserObjectSaveData : SaveData
    {
        [Key(0)]
        public string Id;
        [Key(1)]
        public TransformSaveData Transform;
    }
}