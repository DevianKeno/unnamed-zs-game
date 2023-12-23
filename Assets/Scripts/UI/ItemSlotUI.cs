using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using URMG.Items;
using URMG.Systems;
using URMG.Inventory;

namespace URMG.UI
{
    public class ItemSlotUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
    {
        public static Color Opaque { get => new(1f, 1f, 1f, 1f); }
        public static Color Transparent { get => new(1f, 1f, 1f, 0f); }
        public static Color Normal { get => new(0f, 0f, 0f, 0.5f); }
        public static Color Hovered { get => new(0.2f, 0.2f, 0.2f, 0.5f); }
        
        public int Index;
        public event EventHandler<PointerEventData> OnClick;
        ItemSlot _itemSlot;
        Image _image;
        ItemDisplayUI _displayUI;

        void Awake()
        {
            _image = GetComponent<Image>();
            _displayUI = GetComponentInChildren<ItemDisplayUI>();
        }

        public void OnPointerEnter(PointerEventData e)
        {
            _image.color = Hovered;
        }

        public void OnPointerExit(PointerEventData e)
        {
            _image.color = Normal;
        }

        public void OnPointerDown(PointerEventData e)
        {
            OnClick?.Invoke(this, e);
        }

        public void SetDisplay(Item item)
        {
            _displayUI.SetDisplay(item);
        }
    }
}