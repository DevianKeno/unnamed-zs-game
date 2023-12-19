using System.Collections.Generic;

namespace URMG.InventoryS
{
public struct InventoryData
{
    Slot _primary;
    public Slot Primary { get => _primary; }
    Slot _secondary;
    public Slot Secondary { get => _secondary; }
    List<Slot> _bag;
    public List<Slot> Bag { get => _bag; }
}
}