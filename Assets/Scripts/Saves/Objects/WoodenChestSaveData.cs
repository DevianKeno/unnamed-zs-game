using System;
using System.Collections.Generic;

namespace UZSG.Saves
{
    [Serializable]
    public class WoodenChestSaveData : StorageObjectSaveData
    {
        public List<ItemSlotSaveData> SlotsTwo;
    }
}