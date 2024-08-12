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
        public List<VitalAttributeSaveData> VitalAttributes;
        public List<GenericAttributeSaveData> GenericAttributes;
        public List<string> KnownRecipes;
    }
}
