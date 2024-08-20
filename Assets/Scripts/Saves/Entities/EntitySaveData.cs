using System;
using System.Collections.Generic;

using Newtonsoft.Json;
using JsonSubTypes;

namespace UZSG.Saves
{
    [Serializable]
    [JsonConverter(typeof(JsonSubtypes), "Type")]
    [JsonSubtypes.KnownSubType(typeof(ItemEntitySaveData), "ItemEntitySaveData")]
    [JsonSubtypes.KnownSubType(typeof(PlayerSaveData), "PlayerSaveData")]
    public class EntitySaveData : SaveData
    {
        public int InstanceId;
        public string Id;
        public TransformSaveData Transform;
        public List<AttributeSaveData> Attributes = new();
    }
}
