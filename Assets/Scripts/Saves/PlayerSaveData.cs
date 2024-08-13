using System;
using System.Collections.Generic;

namespace UZSG.Saves
{
    [Serializable]
    public class PlayerSaveData : EntitySaveData
    {
        public InventorySaveData Inventory = new();
        public List<string> KnownRecipes = new();
    }
}
