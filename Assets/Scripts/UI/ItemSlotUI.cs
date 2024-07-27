using System;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

using UZSG.Items;
using UZSG.Inventory;

namespace UZSG.UI
{
    public enum UIState { Normal, Hovered }

    public class ItemSlotUI : Window, ISelectable, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {
        public static Color Opaque { get => new(1f, 1f, 1f, 1f); }
        public static Color Transparent { get => new(1f, 1f, 1f, 0f); }
        public static Color Normal { get => new(0f, 0f, 0f, 0.5f); }
        public static Color Hovered { get => new(0.2f, 0.2f, 0.2f, 0.5f); }
        
        public int Index;

        public event EventHandler<PointerEventData> OnMouseDown;
        public event EventHandler<PointerEventData> OnMouseUp;
        public event EventHandler<PointerEventData> OnStartHover;
        public event EventHandler<PointerEventData> OnEndHover;

        UIState state;

        public Item Item;

        [Space(10)]
        [SerializeField] Image image;
        [SerializeField] TextMeshProUGUI indexTMP;
        [SerializeField] ItemDisplayUI itemDisplayUI;

        void OnValidate()
        {
            if (Application.isPlaying) return;

            if (indexTMP != null)
            {
                indexTMP.text = $"{Index}"; 
            }

            SetDisplayedItem(Item);
        }

        void SetState(UIState state)
        {
            if (state == UIState.Normal)
            {                
                image.color = Normal;
            }
            else if (state == UIState.Hovered)
            {
                image.color = Hovered;
            }
        }


        #region Public methods

        public void SetDisplayedItem(Item item)
        {
            itemDisplayUI?.SetDisplayedItem(item);
        }

        public void Refresh()
        {
            SetDisplayedItem(Item);
        }

        #endregion

        public void OnPointerEnter(PointerEventData e)
        {
            OnStartHover?.Invoke(this, e);
            image.color = Hovered;
        }

        public void OnPointerExit(PointerEventData e)
        {
            OnEndHover?.Invoke(this, e);
            image.color = Normal;
        }

        public void OnPointerDown(PointerEventData e)
        {
            OnMouseDown?.Invoke(this, e);
        }

        public void OnPointerUp(PointerEventData e)
        {
            OnMouseUp?.Invoke(this, e);
        }
    }
}