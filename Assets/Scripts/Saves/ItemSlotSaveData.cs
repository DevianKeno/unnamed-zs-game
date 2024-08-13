using System;
using System.Collections.Generic;

using UZSG.Attributes;

namespace UZSG.Saves
{
    [Serializable]
    public class ItemSlotSaveData : SaveData
    {
        public int Index;
        public ItemSaveData Item;
    }
}