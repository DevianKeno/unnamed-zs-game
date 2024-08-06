using System;
using System.Collections.Generic;
using UZSG.Attributes;
using UZSG.Crafting;

namespace UZSG.Data
{
    [Serializable]
    public class PlayerData
    {
        public InventoryData Inventory;
        public List<VitalAttributeSaveData> Vitals;
        public List<GenericAttributeSaveData> Generic;
    }
}
