using UnityEngine;
using URMG.Items;
using URMG.UI;

namespace URMG.InventoryS
{
/// <summary>
/// Inventory manager.
/// </summary>
public class InventoryHandler
{
    public const int MaxBagSlots = 18;
    InventoryUI ui;
    Slot[] _slots = new Slot[MaxBagSlots];
    Slot[] _occupiedSlots = new Slot[MaxBagSlots];
    Slot[] _emptySlots = new Slot[MaxBagSlots];

    public InventoryHandler()
    {
        Init();
    }

    void Init()
    {
        for (int i = 0; i < _slots.Length; i++)
        {
            _slots[i] = new(i);
        }
    }

    public void BindUI(InventoryUI ui)
    {
        this.ui = ui;
    }

    /// <summary>
    /// Tries to puts the item to the lowest indexed empty slot.
    /// </summary>
    public bool TryPutNearest(Item item)
    {
        foreach (Slot slot in _slots)
        {
            if (slot.IsEmpty)
            {
                slot.SetItem(item);
                ui.SetDisplayedItem(slot.Index, item);
                return true;
            }
            
            if (slot.Item.CompareTo(item))
            {
                if (slot.AddItem(item) != null)
                {
                    return TryPutNearest(item);
                }
                ui.SetDisplayedItem(slot.Index, slot.Item);
                return true;
            } else
            {
                continue;
            }
        }
        return false;
    }

    public void DropItem(int index)
    {

    }
}
}
