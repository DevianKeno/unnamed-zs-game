using URMG.UI;

namespace URMG.Items
{
public interface IComparable<T>
{
    public bool CompareTo(T other);
}

public class Item : IComparable<Item>
{    
    ItemData _itemData;
    public ItemData Data { get => _itemData; }

    int _count;
    public int Count { get => _count; }

    public Item(ItemData itemData, int count)
    {
        _itemData = itemData;
        _count = count;
    }

    public bool CompareTo(Item other)
    {
        if (Data.Id == other.Data.Id) return true;
        return false;
    }
}
}