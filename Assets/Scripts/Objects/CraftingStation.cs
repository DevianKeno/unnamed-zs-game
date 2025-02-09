using System;
using System.Collections.Generic;

using UnityEngine;

using UZSG.Crafting;
using UZSG.Data;
using UZSG.Entities;
using UZSG.Interactions;
using UZSG.Inventory;
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
        public string DisplayName => objectData.DisplayNameTranslatable;
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
        protected virtual void OnCraftEvent(CraftingRoutine routine)
        {
            OnCraft?.Invoke(routine);
        }

        protected virtual void Awake()
        {
            Crafter = GetComponent<Crafter>();
        }

        protected override void OnPlace()
        {
            QueueSlots = new List<ItemSlot>(WorkstationData.QueueSize);
            OutputContainer = new Container(WorkstationData.OutputSize);
            InputContainer = new Container();
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
                if (GUI.IsVisible) CraftingUtils.PlayNoMaterialsSound();
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
    }
}