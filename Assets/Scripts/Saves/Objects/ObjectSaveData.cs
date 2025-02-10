using System;

using Newtonsoft.Json;
using JsonSubTypes;

namespace UZSG.Saves
{
    [Serializable]
    [JsonConverter(typeof(JsonSubtypes), "Type")]
    [JsonSubtypes.KnownSubType(typeof(StorageObjectSaveData), "StorageObjectSaveData")]
    [JsonSubtypes.KnownSubType(typeof(WorkstationObjectSaveData), "WorkstationObjectSaveData")]
    public class ObjectSaveData : SaveData
    {
        public string Id;
        public TransformSaveData Transform;
    }
}