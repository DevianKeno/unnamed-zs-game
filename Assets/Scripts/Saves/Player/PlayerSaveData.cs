using System;
using System.Collections.Generic;

namespace UZSG.Saves
{
    [Serializable]
    public class PlayerSaveData : EntitySaveData
    {
        public string UID;
        public InventorySaveData Inventory = new();
        public List<string> KnownRecipes = new();
        public bool IsCreative;
    }
}
