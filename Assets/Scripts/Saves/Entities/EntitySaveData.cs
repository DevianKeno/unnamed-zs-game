using System;
using System.Collections.Generic;

using Newtonsoft.Json;
using JsonSubTypes;

namespace UZSG.Saves
{
    [Serializable]
    [JsonConverter(typeof(JsonSubtypes), "Type")]
    [JsonSubtypes.KnownSubType(typeof(PlayerSaveData), "PlayerSaveData")]
    [JsonSubtypes.KnownSubType(typeof(PlayerSaveData), "NPCSaveData")]
    [JsonSubtypes.KnownSubType(typeof(ItemEntitySaveData), "ItemEntitySaveData")]
    public class EntitySaveData : SaveData
    {
        public int InstanceId;
        public string Id;
        public TransformSaveData Transform = null;
        public List<AttributeSaveData> Attributes = new();
    }
}
