using UnityEngine;
using MessagePack;

using UZSG.Saves;
using System.Collections.Generic;

namespace UZSG.Worlds
{
    public class WorldSaveData : SaveData
    {
        [Key(0)]
        public List<UserObjectSaveData> UserObjects;
        [Key(1)]
        public List<EntitySaveData> EntitySaves;
        [Key(2)]
        public List<PlayerSaveData> PlayerSaves;
    }

    public class UserObjectSaveData : SaveData
    {
        public TransformSaveData Transform;
    }
}