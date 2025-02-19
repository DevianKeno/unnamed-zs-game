using System;
using System.Collections.Generic;

namespace UZSG.Saves
{
    [Serializable]
    public class PlayerSaveData : EntitySaveData
    {
        public static readonly PlayerSaveData Empty = new();

        public string UID = string.Empty;
        public string DisplayName = string.Empty;
        public InventorySaveData Inventory = new();
        public List<string> KnownRecipeIds = new();
        public bool IsCreative = false;
        public bool IsGodMode = false;

        public static bool IsEmpty(PlayerSaveData psd)
        {
            return psd == Empty;
        }
    }
}
