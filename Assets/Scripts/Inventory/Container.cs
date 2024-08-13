using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

using UZSG.Inventory;
using UZSG.Items;

namespace UZSG
{
    /// <summary>
    /// Base class for all Containers that have ItemSlots.
    /// </summary>
    [Serializable]
    public partial class Container
    {
        protected int _slotCount;
        public int SlotCount
        {
            get => _slotCount;
        }
        [SerializeField] protected List<ItemSlot> _slots = new();
        public List<ItemSlot> Slots => _slots;
        Dictionary<string, HashSet<ItemSlot>> _cachedIdSlots = new();
        /// <summary>
        /// Cached list of ItemSlots of a particular Item present in this Container.
        /// Key is Item Id; Value is all ItemSlots that contains the Item with the same Id.
        /// </summary>
        public Dictionary<string, HashSet<ItemSlot>> IdSlots => _cachedIdSlots;
        Dictionary<string, int> _cachedIdItemCount = new();
        /// <summary>
        /// Cached total count per Item of Id.
        /// Key is Item Id; Value is total count of that Item in this Container.
        /// </summary>
        public Dictionary<string, int> IdItemCount => _cachedIdItemCount;
        public bool IsFull
        {
            get
            {
                foreach (var slot in _slots)
                {
                    if (slot == null) continue;
                    if (slot.IsEmpty) return false;
                }
                return true;
            }
        }
    
        public int FreeSlotsCount
        {
            get
            {
                return _slots.Count(slot => slot.IsEmpty);
            }
        }
    
    
        #region Events

        /// <summary>
        /// Called whenever the Item of a Slot is changed.
        /// </summary>
        public event EventHandler<SlotItemChangedContext> OnSlotItemChanged;

        #endregion


        public ItemSlot this[int i]
        {
            get
            {
                if (!Slots.IsValidIndex(i)) return null;
                return _slots[i];
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="slotsCount"></param>
        public Container(int slotsCount)
        {
            _slotCount = Math.Clamp(slotsCount, 0, 999); /// Max container size
            CreateSlots();
        }

        void CreateSlots()
        {
            for (int i = 0; i < SlotCount; i++)
            {
                ItemSlot newSlot = new(i, ItemSlotType.All);
                newSlot.OnItemChangedInternal += SlotContentChangedInternal;
                _slots.Add(newSlot);
            }
        }

        internal void SlotContentChangedInternal(object sender, ItemSlot.ItemChangedContext context)
        {
            var slot = (ItemSlot) sender;

            if (context.NewItem.IsNone) /// item was removed, and no new Item, thus uncache old item
            {
                UncacheItem(context.OldItem, slot);
            }
            else /// there's a new item
            {
                if (!context.OldItem.IsNone) /// uncache old item if there's any
                {
                    UncacheItem(context.OldItem, slot);
                }
                /// cache new item
                CacheItem(context.NewItem, slot);
            }

            _freeSlotsCounter = 0; /// reset to 0 lewl
            SlotContentChanged(slot, context);
        }

        protected virtual void SlotContentChanged(object sender, ItemSlot.ItemChangedContext context)
        {
            OnSlotItemChanged?.Invoke(this, new()
            {
                Slot = (ItemSlot) sender
            });
        }

        void CacheItem(Item item, ItemSlot slot)
        {
            if (!_cachedIdSlots.TryGetValue(item.Id, out var hashset))
            {
                hashset = new();
                _cachedIdSlots[item.Data.Id] = hashset;
            }
            hashset.Add(slot);

            if (!_cachedIdItemCount.TryGetValue(item.Id, out var count))
            {
                count = 0;
            }
            count += item.Count;
            _cachedIdItemCount[item.Id] = count;
        }

        void UncacheItem(Item item, ItemSlot slot)
        {
            if (_cachedIdSlots.TryGetValue(item.Id, out var hashset))
            {
                hashset.Remove(slot);
                if (hashset.Count == 0)
                {
                    _cachedIdSlots.Remove(item.Id);
                }
            }

            if (_cachedIdItemCount.TryGetValue(item.Id, out var count))
            {
                count -= item.Count;
                if (count <= 0)
                {
                    _cachedIdItemCount.Remove(item.Id);
                }
                else
                {
                    _cachedIdItemCount[item.Id] = count;
                }
            }
        }
        
        public virtual Item ViewItem(int slotIndex)
        {
            if (!Slots.IsValidIndex(slotIndex)) return Item.None;
            return new(Slots[slotIndex].Item);
        }

        public virtual List<ItemSlot> FindItem(string id)
        {
            if (_cachedIdSlots.TryGetValue(id, out var slots))
            {
                return slots.ToList(); 
            }

            return new();
        }

        public virtual List<ItemSlot> FindItem(Item item)
        {
            if (_cachedIdSlots.TryGetValue(item.Data.Id, out var slots))
            {
                return slots.ToList(); 
            }

            return new();
        }

        [Obsolete("You are advised to use TryGetSlot() instead.")]
        public ItemSlot GetSlot(int index)
        {
            if (!Slots.IsValidIndex(index))
            {
                return null;
            }
            return Slots[index];
        }
        
        public bool TryGetSlot(int index, out ItemSlot slot)
        {
            if (!Slots.IsValidIndex(index))
            {
                slot = null;
                return false;
            }
            slot = Slots[index];
            return true;
        }
    }
}
