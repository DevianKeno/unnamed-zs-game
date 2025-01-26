using System;
using System.Collections.Generic;

namespace UZSG.Saves
{
    [Serializable]
    public class InventorySaveData : SaveData
    {
        public List<ItemSlotSaveData> Bag = new();
        public List<ItemSlotSaveData> Hotbar = new();
        public List<ItemSlotSaveData> Equipment = new();
    }
}
