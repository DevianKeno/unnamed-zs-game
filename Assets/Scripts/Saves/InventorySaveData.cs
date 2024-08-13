using System;
using System.Collections.Generic;

namespace UZSG.Saves
{
    [Serializable]
    public class InventorySaveData : SaveData
    {
        public ContainerSaveData Bag;
        public ContainerSaveData Hotbar;
        public ContainerSaveData Equipment;
    }
}
