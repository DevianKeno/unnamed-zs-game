using System.Collections.Generic;

namespace UZSG.Inventory
{
    public struct BagData
    {
        ItemSlot _primary;
        public ItemSlot Primary { get => _primary; }
        ItemSlot _secondary;
        public ItemSlot Secondary { get => _secondary; }
        List<ItemSlot> _bag;
        public List<ItemSlot> Bag { get => _bag; }
    }
}