using System;
using System.Collections.Generic;

using MessagePack;

namespace UZSG.Saves
{
    [Serializable]
    public class EntitySaveData : SaveData
    {
        public string Id;
        public TransformSaveData Transform;
        public List<AttributeSaveData> Attributes = new();
    }
}
