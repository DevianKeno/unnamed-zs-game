using System;
using System.Collections.Generic;

namespace UZSG.Saves
{
    [Serializable]
    public class ContainerSaveData : SaveData
    {
        public List<ItemSlotSaveData> ItemSlots;
    }
}