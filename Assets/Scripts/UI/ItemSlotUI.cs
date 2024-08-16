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

        public enum ClickType {
            Pickup, Split, Clone
        }

        public struct ClickedContext
        {
            public ClickType ClickType { get; set; }
            public PointerEventData PointerEventData { get; set; }
        }
        
        ItemSlot itemSlot;
        public ItemSlot ItemSlot => itemSlot;
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

        [Space]
        [SerializeField] protected Image image;
        [SerializeField] protected TextMeshProUGUI indexTMP;
        [SerializeField] protected ItemDisplayUI itemDisplayUI;
        [SerializeField] protected Button button;


        #region Events

        public event EventHandler<PointerEventData> OnMouseDown;
        public event EventHandler<PointerEventData> OnMouseUp;
        public event EventHandler<PointerEventData> OnHoverStart;
        public event EventHandler<PointerEventData> OnHoverEnd;

        #endregion


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

        public override void OnShow()
        {
            if (itemSlot != null)
            {
                SetDisplayedItem(itemSlot.Item);
            }
        }

        /// <summary>
        /// Connects this Item Slot UI to given ItemSlot.        
        /// </summary>
        public void Link(ItemSlot slot)
        {
            itemSlot = slot;
            slot.OnItemChanged += OnSlotItemChanged;
        }

        void OnSlotItemChanged(object sender, ItemSlot.ItemChangedContext e)
        {
            SetDisplayedItem(e.NewItem);
        }

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