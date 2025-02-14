using System;
using System.Collections.Generic;

using JsonSubTypes;
using Newtonsoft.Json;

namespace UZSG.Saves
{
    [Serializable]
    [JsonConverter(typeof(JsonSubtypes), "Type")]
    [JsonSubtypes.KnownSubType(typeof(PlayerSaveData), "PlayerSaveData")]
    [JsonSubtypes.KnownSubType(typeof(PlayerSaveData), "NPCSaveData")]
    [JsonSubtypes.KnownSubType(typeof(EnemySaveData), "EnemySaveData")]
    [JsonSubtypes.KnownSubType(typeof(ItemEntitySaveData), "ItemEntitySaveData")]
    public class EntitySaveData : SaveData
    {
        public TransformSaveData Transform = new();
        public List<AttributeSaveData> Attributes = new();
    }
}
