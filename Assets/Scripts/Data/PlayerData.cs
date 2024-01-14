using System;
using UZSG.Attributes;

namespace UZSG.Data
{
    [Serializable]
    public class PlayerData
    {
        public InventoryData Inventory;
        public AttributeCollectionData<VitalAttribute> Vitals;
        public AttributeCollectionData<GenericAttribute> Generic;
    }
}
