using System;
using System.Collections.Generic;

namespace UZSG.Saves
{
    [Serializable]
    public class PlayerSaveData : SaveData
    {
        /// Attributes first
        public AttributeCollectionSaveData VitalAttributes;
        public AttributeCollectionSaveData GenericAttributes;
        
        public InventorySaveData Inventory;
        public List<string> KnownRecipes;
    }
}
