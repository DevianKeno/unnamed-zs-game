using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

using UZSG.Saves;
using UZSG.Inventory;
using UZSG.Items;

namespace UZSG
{
    /// <summary>
    /// Base class for all Containers that have ItemSlots.
    /// </summary>
    [Serializable]
    public partial class Container : ISaveDataReadWrite<List<ItemSlotSaveData>>
    {
        public const int MAX_CONTAINER_SIZE = short.MaxValue;

        protected int slotCount;
        public int SlotCount
        {
            get => slotCount;
        }
        [SerializeField] protected List<ItemSlot> slots = new();
        public List<ItemSlot> Slots => slots;
        Dictionary<string, HashSet<ItemSlot>> cachedIdSlots = new();
        /// <summary>
        /// Cached list of ItemSlots of a particular Item present in this Container.
        /// Key is Item Id; Value is all ItemSlots that contains the Item with the same Id.
        /// </summary>
        public Dictionary<string, HashSet<ItemSlot>> IdSlots => cachedIdSlots;
        Dictionary<string, int> cachedIdItemCount = new();
        /// <summary>
        /// Cached total count per Item of Id.
        /// Key is Item Id; Value is total count of that Item in this Container.
        /// </summary>
        public Dictionary<string, int> IdItemCount => cachedIdItemCount;
        /// <summary>
        /// TODO: was planning to implement "caching" on this since this still just loops over all the slots
        /// </summary>
        public bool IsFull 
        {
            get
            {
                return FreeSlotsCount == 0;
            }
        }
        /// <summary>
        /// Whether the container has any items or not.
        /// </summary>
        public bool HasAnyItem
        {
            get
            {
                return cachedIdSlots.Any();
            }
        }
        public int FreeSlotsCount
        {
            get
            {
                return slots.Count(slot => slot.IsEmpty);
            }
        }
    
    
        #region Events

        /// <summary>
        /// Raised whenever the Item of a Slot is changed.
        /// </summary>
        public event EventHandler<ItemSlot.ItemChangedContext> OnSlotItemChanged;

        #endregion


        public ItemSlot this[int i]
        {
            get
            {
                if (!Slots.IsValidIndex(i)) return null;
                return slots[i];
            }
        }

        public Container(int slotsCount = 0)
        {
            this.slotCount = Math.Clamp(slotsCount, 0, MAX_CONTAINER_SIZE);
            
            for (int i = 0; i < SlotCount; i++)
            {
                var newSlot = new ItemSlot(i, ItemSlotType.All);
                newSlot.OnItemChangedInternal += SlotContentChangedInternal;
                slots.Add(newSlot);
            }
        }

        /// <summary>
        /// Experimental
        /// </summary>
        public void AddSlots(int count)
        {
            int addedSlotCount = Math.Clamp(count, 0, MAX_CONTAINER_SIZE - SlotCount);
            this.slotCount += addedSlotCount;
            
            for (int i = 0; i < addedSlotCount; i++)
            {
                var newSlot = new ItemSlot(i, ItemSlotType.All);
                newSlot.OnItemChangedInternal += SlotContentChangedInternal;
                slots.Add(newSlot);
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

            SlotContentChanged(slot, context);
        }

        protected virtual void SlotContentChanged(object sender, ItemSlot.ItemChangedContext context)
        {
            OnSlotItemChanged?.Invoke(this, context);
        }

        void CacheItem(Item item, ItemSlot slot)
        {
            if (item.IsNone) return;
            
            if (!cachedIdSlots.TryGetValue(item.Id, out var hashset))
            {
                hashset = new();
                cachedIdSlots[item.Data.Id] = hashset;
            }
            hashset.Add(slot);

            if (!cachedIdItemCount.TryGetValue(item.Id, out var count))
            {
                count = 0;
            }
            count += item.Count;
            cachedIdItemCount[item.Id] = count;
        }

        void UncacheItem(Item item, ItemSlot slot)
        {
            if (item.IsNone) return;

            if (cachedIdSlots.TryGetValue(item.Id, out var hashset))
            {
                hashset.Remove(slot);
                if (hashset.Count == 0)
                {
                    cachedIdSlots.Remove(item.Id);
                }
            }

            if (cachedIdItemCount.TryGetValue(item.Id, out var count))
            {
                count -= item.Count;
                if (count <= 0)
                {
                    cachedIdItemCount.Remove(item.Id);
                }
                else
                {
                    cachedIdItemCount[item.Id] = count;
                }
            }
        }


        #region Public methods
        
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

        public ItemSlot GetNearestEmptySlot()
        {
            if (IsFull) return null;

            foreach (var slot in Slots)
            {
                if (slot.IsEmpty) return slot;
            }

            return null;
        }

        /// <summary>
        /// Extends this Container with another Container.
        /// <b>This is irreversible!</b>
        /// </summary>
        public void Extend(Container other)
        {
            var slots = new List<ItemSlot>();
            slots.AddRange(Slots);

            foreach (var slot in other.Slots) /// Subscribe the new Slots to Container events
            {
                if (!slot.IsEmpty) CacheItem(slot.Item, slot);
                slot.OnItemChangedInternal += SlotContentChangedInternal;
            }

            slots.AddRange(other.Slots); /// OGs are already subscribed :)
            this.slots = slots;
        }

        public void ReadSaveData(List<ItemSlotSaveData> saveData)
        {
            if (saveData == null) return;
            
            if (saveData.Count > SlotCount)
            {
                Game.Console.LogWarn($"ContainerSaveData has Items greater than the Container's capacity. You might lose some items.");
            }
            
            foreach (var sd in saveData)
            {
                if (TryGetSlot(sd.Index, out var slot))
                {
                    slot.Put(new Item(sd.Item.Id, sd.Item.Count));
                }
            }
        }

        public virtual List<ItemSlotSaveData> WriteSaveData()
        {
            var saveData = new List<ItemSlotSaveData>();

            foreach (var s in slots)
            {
                if (s.IsEmpty) continue; /// don't save empty slots
                
                var slotSd = s.WriteSaveData();
                saveData.Add(slotSd);
            }

            return saveData;

        }

        #endregion
    }
}
