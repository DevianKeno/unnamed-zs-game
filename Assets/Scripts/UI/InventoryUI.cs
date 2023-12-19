using System.Collections.Generic;
using UnityEngine;
using URMG.Items;

namespace URMG.UI
{
public class InventoryUI : MonoBehaviour
{
    public const int MaxBagSlots = 18;
    Dictionary<int, SlotUI> slots = new();
    [SerializeField] GameObject bag;
    bool _isVisible = true;

    void Awake()
    {
        int i = 0;
        foreach (Transform t in bag.transform)
        {
            slots.Add(i, t.GetComponent<SlotUI>());
            i++;
        }
    }

    public void SetDisplayedItem(int slotIndex, Item item)
    {
        if (slotIndex > MaxBagSlots )
        {
            Debug.Log("Slot index out of bounds.");
            return;
        }
        slots[slotIndex].SetDisplayItem(item);
    }

    public void RefreshSlot(int index)
    {
        slots[index].Refresh();
    }

    public void Show()
    {
        if (_isVisible) return;
        _isVisible = true;
        gameObject.SetActive(true);
        Cursor.Show();
    }

    public void Hide()
    {
        if (!_isVisible) return;
        _isVisible = false;
        gameObject.SetActive(false);
        Cursor.Hide();
    }

    public void ToggleVisibility()
    {
        if (_isVisible)
        {
            Hide();
        } else
        {
            Show();
        }
    }
}
}