using System;
using System.Collections.Generic;

using JsonSubTypes;
using Newtonsoft.Json;

namespace UZSG.Saves
{
    [Serializable]
    [JsonConverter(typeof(JsonSubtypes), "Type")]
    [JsonSubtypes.KnownSubType(typeof(StorageObjectSaveData), "StorageObjectSaveData")]
    [JsonSubtypes.KnownSubType(typeof(WorkstationObjectSaveData), "WorkstationObjectSaveData")]
    public class BaseObjectSaveData : SaveData
    {
        public TransformSaveData Transform;
        public List<AttributeSaveData> Attributes = new();
    }
}