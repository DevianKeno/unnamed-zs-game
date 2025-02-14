using System;
using System.Collections.Generic;

namespace UZSG.Saves
{
    [Serializable]
    public class CraftingStationSaveData : BaseObjectSaveData
    {
        public List<CraftingRoutineSaveData> CraftingRoutines = new();
        public List<ItemSlotSaveData> OutputSlots = new();
    }
}