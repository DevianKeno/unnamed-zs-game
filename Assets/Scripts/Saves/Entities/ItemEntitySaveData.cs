using System;
using Newtonsoft.Json;

namespace UZSG.Saves
{
    [Serializable]
    public class ItemEntitySaveData : EntitySaveData
    {
        public ItemSaveData Item;
        public int Age;
    }
}