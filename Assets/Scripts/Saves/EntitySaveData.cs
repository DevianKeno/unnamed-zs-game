using System;
using System.Collections.Generic;

namespace UZSG.Saves
{
    [Serializable]
    public class EntitySaveData : SaveData
    {
        /// Attributes first
        public List<AttributeSaveData> Attributes = new();
    }
}
