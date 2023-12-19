using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using URMG.Items;
using URMG.Core;
using URMG.UI.Colors;
using System;

namespace URMG.UI
{
    public class SlotUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
    {
        public static Color Opaque { get => new(1f, 1f, 1f, 1f); }
        public static Color Transparent { get => new(1f, 1f, 1f, 0f); }
        public static Color Normal { get => new(0f, 0f, 0f, 0.5f); }
        public static Color Hovered { get => new(0.2f, 0.2f, 0.2f, 0.5f); }
    
        ItemDisplayUI displayUI;
        [SerializeField] Image image;
        public event EventHandler<PointerEventData> OnClick;

        public void OnPointerEnter(PointerEventData e)
        {
            image.color = Hovered;
        }

        public void OnPointerExit(PointerEventData e)
        {
            image.color = Normal;
        }

        public void OnPointerDown(PointerEventData e)
        {
            OnClick?.Invoke(this, e);
        }

        public void SetDisplayUI(ItemDisplayUI obj)
        {
            displayUI = obj;
        }

        public void SetDisplay(Item item)
        {
            if (displayUI == null) return;
            displayUI.SetDisplay(item);
        }
    }
}