using URMG.Items;

namespace URMG.InventoryS
{
public class Slot
{    
    int _index;
    public int Index { get => _index; }

    Item _item;
    public Item Item { get => _item; }

    bool _isEmpty;
    public bool IsEmpty { get => _isEmpty; }

    public Slot(int index)
    {
        _index = index;
        _item = null;
        _isEmpty = true;
    }

    public void SetItem(Item item)
    {
        _item = item;
        _isEmpty = false;
    }

    /// <summary>
    /// Returns excess items if any.
    /// </summary>
    public Item AddItem(Item toAdd)
    {
        if (!toAdd.Data.IsStackable) return toAdd;

        int newCount = _item.Count + toAdd.Count;
        int excess = newCount - _item.Data.MaxStackSize;

        if (excess > 0)
        {
            _item = new(_item.Data, _item.Data.MaxStackSize);
            return new(_item.Data, excess);
        } else
        {
            _item = new(_item.Data, newCount);
            return null;
        }
    }
}
}