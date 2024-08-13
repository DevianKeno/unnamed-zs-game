using System;
using System.Collections.Generic;

namespace UZSG.Saves
{
    [Serializable]
    public class PlayerSaveData : SaveData
    {
        /// Attributes first
        public List<AttributeSaveData> Attributes = new();
        public InventorySaveData Inventory = new();
        public List<string> KnownRecipes = new();
    }
}
