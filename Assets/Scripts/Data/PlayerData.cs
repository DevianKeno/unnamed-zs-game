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
        public List<AttributeSaveData> Vitals;
        public List<AttributeSaveData> Generic;
    }
}
