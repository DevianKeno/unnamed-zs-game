using System;

using UnityEngine;

using UZSG.Crafting;
using UZSG.Entities;
using UZSG.Players;

namespace UZSG.UI
{
    public class PlayerCraftingGUI : CraftingGUI
    {
        public PlayerCrafting PlayerCrafting
        {
            get => (PlayerCrafting) CraftingStation;
            protected set
            {
                base.CraftingStation = value;
            }
        }

        public void SetPlayer(Player player, PlayerCrafting playerCrafting)
        {
            Player = player;
            PlayerCrafting = playerCrafting;

            Player.Inventory.Bag.OnSlotItemChanged += OnPlayerBagItemChanged;
            PlayerCrafting.OnCraft += OnWorkstationCraft;

            CreateQueueSlotUIs(6); /// attributeable
            CreateOutputSlotUIs(6); /// attributeable
        }

        protected override void OnShow()
        {
            ResetDisplayed();
            AddRecipesById(Player.SaveData.KnownRecipeIds); /// by raw Recipe Id. ID!!!!!!
            /// Retrieve crafting routines
            foreach (var r in PlayerCrafting.Crafter.Routines)
            {
                CreateRoutineProgressUI(r);
            }
        }

        protected override void OnHide()
        {
            if (CraftingStation != null)
            {
                CraftingStation.OnCraft -= OnWorkstationCraft;
            }
            
            foreach (var ui in routineUIs.Values)
            {
                Destroy(ui.gameObject);
            }
            routineUIs.Clear();
        }
    }
}