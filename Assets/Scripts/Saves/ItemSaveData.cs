using System;
using System.Collections.Generic;

using UZSG.Attributes;

namespace UZSG.Saves
{
    [Serializable]
    public class ItemSaveData : SaveData
    {
        public string Id;
        public int Count;
        public AttributeCollectionSaveData Attributes;
    }
}