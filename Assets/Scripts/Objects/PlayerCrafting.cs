using System;
using System.Collections.Generic;

using UnityEngine;

using UZSG.Crafting;
using UZSG.Entities;
using UZSG.Inventory;
using UZSG.UI;

namespace UZSG.Players
{
    [RequireComponent(typeof(Crafter))]
    public class PlayerCrafting : MonoBehaviour
    {
        public Player Player { get; set; }
        
        public Crafter Crafter { get; protected set; }
        public Container InputContainer { get; protected set; }
        public Container OutputContainer { get; protected set; }
        public List<ItemSlot> QueueSlots { get; protected set; }
        public CraftingGUI GUI { get; protected set; }
        /// <summary>
        /// Called when this workstation has crafted, either single or complete.
        /// </summary>
        public event Action<CraftingRoutine> OnCraft;

        void Awake()
        {
            Crafter = GetComponent<Crafter>();
        }

        internal void Initialize(Player player, bool isLocalPlayer)
        {
            this.Player = player;

            QueueSlots = new(1);
            OutputContainer = new(1);
            InputContainer = new Container();
            // OutputContainer.OnSlotItemChanged += OnOutputSlotItemChanged;

            // Crafter.Initialize(this);
            Crafter.OnRoutineNotify += OnCraftingRoutineNotify;

            if (isLocalPlayer)
            {
                GUI = player.InventoryWindow.PlayerCraftingGUI;
                GUI.SetPlayer(Player);
                GUI.Show();
            }

            InputContainer.Extend(player.Inventory.Bag);
        }

        void OnCraftingRoutineNotify(CraftingRoutine routine)
        {
            switch (routine.Status)
            {
                case CraftingRoutineStatus.Ongoing:
                {
                    
                    break;
                }
            }
        }

        public CraftingRoutine TryCraft(ref CraftItemOptions options)
        {
            if (Crafter.IsQueueFull)
            {
                return null;
            }

            var totalMaterials = CraftingUtils.CalculateTotalMaterials(options);
            if (false == this.Player.Inventory.Bag.ContainsAll(totalMaterials))
            {
                if (GUI.IsVisible) 
                {
                    Game.Audio.PlayInUI("insufficient_materials");
                }
                return null;
            }

            _ = InputContainer.TakeItems(totalMaterials);
            return Crafter.CraftNewItem(ref options);
        }

        protected virtual void OnCraftEvent(CraftingRoutine routine)
        {
            OnCraft?.Invoke(routine);
        }
    }
}