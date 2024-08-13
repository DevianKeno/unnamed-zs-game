using System;
using System.Collections.Generic;

namespace UZSG.Saves
{
    [Serializable]
    public class AttributeCollectionSaveData : SaveData
    {
        public List<AttributeSaveData> Attributes;
    }
}