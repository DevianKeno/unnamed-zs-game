using System;
using System.Collections.Generic;

namespace UZSG.Saves
{
    [Serializable]
    public class InventorySaveData : SaveData
    {
        public List<ItemSlotSaveData> Bag;
        public List<ItemSlotSaveData> Hotbar;
        public List<ItemSlotSaveData> Equipment;
    }
}
