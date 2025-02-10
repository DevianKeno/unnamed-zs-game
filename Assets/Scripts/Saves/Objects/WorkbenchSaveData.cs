using System;
using System.Collections.Generic;

namespace UZSG.Saves
{
    [Serializable]
    public class WorkbenchSaveData : BaseObjectSaveData
    {
        public List<CraftingRoutineSaveData> craftingRoutines = new();
        public List<ItemSlotSaveData> QueueSlots = new();
        public List<ItemSlotSaveData> OutputSlots = new();
    }
}