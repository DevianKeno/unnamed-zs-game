using System;
using System.Collections.Generic;
using UZSG.Attributes;
using UZSG.Crafting;

namespace UZSG.Data
{
    [Serializable]
    public struct PlayerSaveData
    {
        public InventorySaveData Inventory;
        public List<string> KnownRecipes;
        public List<VitalAttributeSaveData> VitalAttributes;
        public List<GenericAttributeSaveData> GenericAttributes;
    }
}
