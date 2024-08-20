using System;
using System.Collections.Generic;

namespace UZSG.Saves
{
    [Serializable]
    public class StorageObjectSaveData : ObjectSaveData
    {
        public List<ItemSlotSaveData> Slots;
    }
}