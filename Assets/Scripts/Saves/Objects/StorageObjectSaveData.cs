using System;
using System.Collections.Generic;

namespace UZSG.Saves
{
    [Serializable]
    public class StorageObjectSaveData : BaseObjectSaveData
    {
        public List<ItemSlotSaveData> Slots;
    }
}