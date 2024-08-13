using System;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

using UZSG.Items;
using UZSG.Inventory;

namespace UZSG.UI
{
    public class ItemSlotUI : Window, ISelectable, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {
        public static Color Opaque => new(1f, 1f, 1f, 1f);
        public static Color Transparent => new(1f, 1f, 1f, 0f);
        public static Color Normal => new(0f, 0f, 0f, 0.5f);
        public static Color Hovered => new(0.2f, 0.2f, 0.2f, 0.5f);
        
        [SerializeField] protected Item item = Item.None;
        public Item Item
        {
            get
            {
                return item;
            }
            set
            {
                SetDisplayedItem(item);
            }
        }
        public int Index;


        #region Events

        public event EventHandler<PointerEventData> OnMouseDown;
        public event EventHandler<PointerEventData> OnMouseUp;
        public event EventHandler<PointerEventData> OnHoverStart;
        public event EventHandler<PointerEventData> OnHoverEnd;

        #endregion


        [Space]
        [SerializeField] protected Image image;
        [SerializeField] protected TextMeshProUGUI indexTMP;
        [SerializeField] protected ItemDisplayUI itemDisplayUI;

        protected virtual void OnValidate()
        {
            if (Application.isPlaying) return;

            if (indexTMP != null)
            {
                indexTMP.text = $"{Index}"; 
            }

            SetDisplayedItem(item);
        }

        void OnDisable()
        {
            Reset();
        }


        #region Public methods

        public void SetDisplayedItem(Item item)
        {
            this.item = item;
            itemDisplayUI.SetDisplayedItem(item);
        }

        public void Reset()
        {
            image.color = Normal;
        }

        public void Refresh()
        {
            Reset();
            SetDisplayedItem(Item);
        }

        #endregion


        public void OnPointerEnter(PointerEventData e)
        {
            image.color = Hovered;
            OnHoverStart?.Invoke(this, e);
        }

        public void OnPointerExit(PointerEventData e)
        {
            Reset();
            OnHoverEnd?.Invoke(this, e);
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