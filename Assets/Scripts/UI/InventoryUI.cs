using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using URMG.Core;
using URMG.InventoryS;
using URMG.Items;

namespace URMG.UI
{
public class InventoryUI : MonoBehaviour
{
    public const int MaxBagSlots = 18;
    Dictionary<int, SlotUI> uiSlots = new();
    bool _isVisible = true;
    bool isHolding;
    SlotUI selectedSlot;
    SlotUI heldSlot;
    [SerializeField] GameObject bag;
    [SerializeField] GameObject slotPrefab;
    [SerializeField] GameObject itemDisplayPrefab;

    void Awake()
    {
        InitBag();
    }

    void InitBag()
    {
        for (int i = 0; i < MaxBagSlots; i++)
        {
            GameObject slotGO = Instantiate(slotPrefab, bag.transform);
            slotGO.name = $"Slot {i}";
            GameObject idGO = Instantiate(itemDisplayPrefab, slotGO.transform);
            idGO.name = $"Item Display";

            SlotUI slot = slotGO.GetComponent<SlotUI>();
            slot.SetDisplayUI(idGO.GetComponent<ItemDisplayUI>());
            slot.OnClick += OnClickSlot;
            uiSlots.Add(i, slot);
        }
    }

    public void OnClickSlot(object slot, PointerEventData e)
    {
        selectedSlot = (SlotUI) slot;
        if (e.button == PointerEventData.InputButton.Left)
        {
            if (isHolding)
            {

            } else
            {
                heldSlot = Instantiate(slotPrefab, Game.UI.Canvas.transform).GetComponent<SlotUI>();
            }

        } else if (e.button == PointerEventData.InputButton.Right)
        {
            
        }
    }

    void Update()
    {
        if (heldSlot != null)
        {
            heldSlot.transform.position = Input.mousePosition;
        }
    }

    public void SetDisplayedItem(int slotIndex, Item item)
    {
        if (slotIndex > MaxBagSlots )
        {
            Debug.Log("Slot index out of bounds.");
            return;
        }
        uiSlots[slotIndex].SetDisplay(item);
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