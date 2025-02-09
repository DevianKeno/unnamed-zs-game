using System;

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

using TMPro;

using UZSG.Items;
using UZSG.Inventory;

namespace UZSG.UI
{
    public class ItemSlotUI : UIElement, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {
        public static Color Opaque => new(1f, 1f, 1f, 1f);
        public static Color Transparent => new(1f, 1f, 1f, 0f);
        public static Color Normal => new(0f, 0f, 0f, 0.5f);
        public static Color Hovered => new(0.2f, 0.2f, 0.2f, 0.5f);

        public enum ClickType {
            Pickup, Split, ShiftClick, Clone
        }

        public struct ClickedContext
        {
            public ClickType ClickType { get; set; }
            public PointerEventData Pointer { get; set; }
            public readonly PointerEventData.InputButton Button => Pointer.button;
        }
        
        protected ItemSlot slot;
        public ItemSlot Slot => slot;
        [SerializeField] protected Item item = Item.None;
        public Item Item
        {
            get => item;
            set
            {
                SetDisplayedItem(item);
            }
        }
        [SerializeField] protected int Index;

        [Space]
        [SerializeField] protected Image image;
        [SerializeField] protected TextMeshProUGUI indexTMP;
        [SerializeField] protected ItemDisplayUI itemDisplayUI;
        [SerializeField] protected Button button;


        #region Events

        public event EventHandler<ClickedContext> OnMouseDown,
            OnMouseUp,
            OnHoverStart, 
            OnHoverEnd;

        #endregion


        #region Event callbacks

        protected override void OnValidate()
        {
            if (Application.isPlaying) return;

            if (indexTMP != null)
            {
                indexTMP.text = $"{Index}"; 
            }

            SetDisplayedItem(item);
        }

        public void OnPointerEnter(PointerEventData e)
        {
            var context = new ClickedContext()
            {
                Pointer = e
            };

            image.color = Hovered;
            OnHoverStart?.Invoke(this, context);
        }

        public void OnPointerExit(PointerEventData e)
        {
            Reset();
            var context = new ClickedContext()
            {
                Pointer = e
            };
            OnHoverEnd?.Invoke(this, context);
        }

        public void OnPointerDown(PointerEventData e)
        {
            var context = new ClickedContext()
            {
                Pointer = e
            };

            if (Input.GetKey(KeyCode.LeftShift))
            {
                context.ClickType = ClickType.ShiftClick;
            }
            else
            {
                context.ClickType = ClickType.Pickup;
            }

            OnMouseDown?.Invoke(this, context);
        }

        public void OnPointerUp(PointerEventData e)
        {
            var context = new ClickedContext()
            {
                Pointer = e
            };

            if (Input.GetKey(KeyCode.LeftShift))
            {
                context.ClickType = ClickType.ShiftClick;
            }
            else
            {
                context.ClickType = ClickType.Pickup;
            }
            
            OnMouseUp?.Invoke(this, context);
        }

        void OnSlotItemChanged(object sender, ItemSlot.ItemChangedContext e)
        {
            SetDisplayedItem(e.NewItem);
        }

        void OnDisable()
        {
            Reset();
        }

        void OnDestroy()
        {
            if (slot != null)
            {
                slot.OnItemChanged -= OnSlotItemChanged;
            }
        }

        #endregion

        #region Public methods

        protected override void OnShow()
        {
            if (slot != null)
            {
                SetDisplayedItem(slot.Item);
            }
        }

        /// <summary>
        /// Connects this Item Slot UI to given ItemSlot.        
        /// </summary>
        public void Link(ItemSlot slot)
        {
            this.slot = slot;
            this.Index = slot.Index;
            slot.OnItemChanged += OnSlotItemChanged;

            if (!slot.IsEmpty)
            {
                SetDisplayedItem(slot.Item);
            }
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
    }
}