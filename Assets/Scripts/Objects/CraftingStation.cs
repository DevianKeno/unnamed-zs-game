using System;
using System.Collections.Generic;

using UnityEngine;

using UZSG.Crafting;
using UZSG.Data;
using UZSG.Entities;
using UZSG.Interactions;
using UZSG.Inventory;
using UZSG.Items;
using UZSG.UI;

namespace UZSG.Objects
{
    /// <summary>
    /// Represents objects that can craft items.
    /// </summary>
    [RequireComponent(typeof(Crafter))]
    public class CraftingStation : BaseObject, IPlayerInteractable, ICrafter
    {
        public WorkstationData WorkstationData => objectData as WorkstationData;
        
        public Player Player { get; set; }
        public bool AllowInteractions { get; set; } = false;
        
        public Crafter Crafter { get; protected set; }
        public Container InputContainer { get; protected set; }
        public Container OutputContainer { get; protected set; }
        public List<ItemSlot> QueueSlots { get; protected set; }
        public CraftingGUI GUI { get; protected set; }

        /// <summary>
        /// Called when this workstation has crafted, either single or complete.
        /// </summary>
        public event Action<CraftingRoutine> OnCraft;
        
        protected virtual void Awake()
        {
            Crafter = GetComponent<Crafter>();
        }

        protected override void OnPlaceEvent()
        {
            QueueSlots = new List<ItemSlot>(WorkstationData.QueueSize);
            OutputContainer = new Container(WorkstationData.OutputSize);
            InputContainer = new Container();
            OutputContainer.OnSlotItemChanged += OnOutputSlotItemChanged;
        }
        
        public virtual List<InteractAction> GetInteractActions() { return new(); }
        public virtual void Interact(InteractionContext context) { }

        public virtual bool TryCraft(ref CraftItemOptions options)
        {
            if (Player is not Player player ||
                Crafter.IsQueueFull)
            {
                return false;
            }

            var totalMaterials = CraftingUtils.CalculateTotalMaterials(options);
            if (!player.Inventory.Bag.ContainsAll(totalMaterials))
            {
                CraftingUtils.PlayNoMaterialsSound(this);
                return false;
            }

            _ = InputContainer.TakeItems(totalMaterials);
            Crafter.CraftNewItem(ref options);
            return true;
        }
        
        public void AddInputContainer(Container other)
        {
            this.InputContainer.Extend(other);
        }

        public void SetOutputContainer(Container other)
        {
            this.OutputContainer = other;
        }

        protected void ClearInputContainers()
        {
            InputContainer = new();
        }

        public void CancelCraft(int routineIndex)
        {
            Crafter.CancelRoutine(routineIndex);
        }

        public void CancelCraft(CraftingRoutine routine)
        {
            throw new NotImplementedException();
        }

        protected virtual void OnCraftEvent(CraftingRoutine routine)
        {
            OnCraft?.Invoke(routine);
        }

        /*
            The code snippet below is responsible for making routines wait in queue when the output slots are full.
        */
        /// <summary>
        /// Listens to all output slots when their Item is changed.
        /// </summary>
        protected void OnOutputSlotItemChanged(object sender, ItemSlot.ItemChangedContext e)
        {
            onOutputSlotItemChanged?.Invoke(e);
        }
        protected event Action<ItemSlot.ItemChangedContext> onOutputSlotItemChanged;
        protected virtual void OnCraftSingle(CraftingRoutine routine)
        {
            var outputItem = new Item(routine.RecipeData.Output);
            
            if (OutputContainer.TryPutNearest(outputItem))
            {
                CraftingUtils.PlayCraftSound(this);
                OnCraftEvent(routine);
                return;
            }

            /// output slot is full wtf?? what do lmao
            onOutputSlotItemChanged += PutItemWhenOutputSlotIsEmpty;
            void PutItemWhenOutputSlotIsEmpty(ItemSlot.ItemChangedContext context)
            {
                /// look for empty space
                if (!context.NewItem.Is(Item.None)) return;
                
                onOutputSlotItemChanged -= PutItemWhenOutputSlotIsEmpty;
                context.ItemSlot.Put(outputItem);
                CraftingUtils.PlayCraftSound(this);
                routine.Finish();
                OnCraftEvent(routine);
            };
        }
    }
}