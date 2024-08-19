using System;
using System.Collections.Generic;

using MessagePack;

namespace UZSG.Saves
{
    [Serializable]
    public class EntitySaveData : SaveData
    {
        public TransformSaveData Transform;
        
        /// Attributes first
        [Key(3)]
        public List<AttributeSaveData> Attributes = new();
    }
}
