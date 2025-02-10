using System;
using System.Collections.Generic;

using UnityEngine;

using UZSG.Entities;
using UZSG.Inventory;
using UZSG.Objects;
using UZSG.UI;

namespace UZSG.Crafting
{
    [RequireComponent(typeof(Crafter))]
    public class PlayerCrafting : CraftingStation
    {
        public PlayerCraftingGUI PlayerCraftingGUI => (PlayerCraftingGUI) base.GUI;

        protected override void Awake()
        {
            base.Awake();
        }

        internal void Initialize(Player player, bool isLocalPlayer)
        {
            this.Player = player;

            QueueSlots = new List<ItemSlot>(6);
            OutputContainer = new Container(6);
            InputContainer = new Container();
            
            Crafter.OnRoutineNotify += OnCraftingRoutineNotify;

            if (isLocalPlayer)
            {
                GUI = player.InventoryWindow.CraftingGUI;
                PlayerCraftingGUI.SetPlayer(Player, this);
            }

            InputContainer.Extend(player.Inventory.Bag);
        }

        protected virtual void OnCraftingRoutineNotify(CraftingRoutine routine)
        {
            switch (routine.Status)
            {
                case CraftingRoutineStatus.Ongoing:
                {
                    
                    break;
                }
            }
        }
    }
}