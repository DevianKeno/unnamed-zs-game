using System;
using System.Collections.Generic;

using UnityEditor.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

using UZSG.Crafting;
using UZSG.Items;
using UZSG.Systems;
using UnityEngine.UI;
using UZSG.Data;
using UZSG.Inventory;
using UZSG.Entities;

namespace UZSG.UI
{
    public class CraftingGUI : WorkstationGUI
    {
        Player player;

        RecipeData _selectedRecipe;
        Dictionary<string, CraftableItemUI> craftableItemUIs = new();
        List<MaterialSlotUI> materialSlotUIs = new();

        [SerializeField] CraftedItemDisplayUI craftedItemDisplay;
        [SerializeField] RectTransform craftablesHolder;
        [SerializeField] RectTransform materialSlotsHolder;
        [SerializeField] GameObject outputSlots;
        [SerializeField] Button craftButton;

        /// <summary>
        /// Called when user clicks on a recipe, also referred here as a "Craftable Item".
        /// </summary>
        void OnClickCraftable(CraftableItemUI craftable)
        {
            var selectedRecipe = craftable.RecipeData;
            craftedItemDisplay.SetDisplayedRecipe(selectedRecipe);
            DisplayMaterials(selectedRecipe);

            if (PlayerHasMaterialsFor(selectedRecipe))
            {
                craftButton.interactable = true;
            }
            else
            {
                /// TODO, still interactable but prompt "Insufficient materials"
                craftButton.interactable = false;
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate(transform as RectTransform);
        }

        public void ResetDisplayed()
        {
            craftedItemDisplay.ResetDisplayed();
            ClearCraftableItems();
            ClearMaterialSlots();
        }

        public override void OnShow()
        {
            ResetDisplayed();

            craftButton.onClick.AddListener(() =>
            {
                CraftItem();
            });
        }

        public override void OnHide()
        {
            Unsubscribe();
        }

        void CraftItem()
        {
            // crafter.CraftItem(_selectedRecipe);
        }

        public void SetPlayer(Player player)
        {
            this.player = player;
            // InitializePlayerEvents()
            player.Inventory.Bag.OnSlotItemChanged += OnPlayerBagItemChanged;
        }

        void OnPlayerBagItemChanged(object sender, SlotItemChangedContext e)
        {
            //{
                /// Update craftable list status
                foreach (CraftableItemUI craftable in craftableItemUIs.Values)
                {
                    UpdateCraftableStatusForPlayer(craftable);
                }

                /// Update displayed material count
                foreach (var matSlot in materialSlotUIs)
                {
                    if (player.Inventory.Bag.IdItemCount.TryGetValue(matSlot.Item.Id, out int count))
                    {
                        matSlot.Present = count;
                    }
                    else
                    {
                        matSlot.Present = 0;
                    }
                }
            //}
        }


        #region Craftables Item UIs

        /// <summary>
        /// Add recipes to be displayed by Recipe Id.
        /// </summary>
        public void AddRecipesById(List<string> ids)
        {
            foreach (var id in ids)
            {
                if (Game.Recipes.TryGetRecipeData(id, out var data))
                CreateCraftableItemUI(data);
            }
            LayoutRebuilder.ForceRebuildLayoutImmediate(transform as RectTransform);
        }

        /// <summary>
        /// Add recipes to be displayed by Recipe Data.
        /// </summary>
        public void AddRecipes(List<RecipeData> recipes)
        {
            foreach (var recipe in recipes)
            {
                CreateCraftableItemUI(recipe);
            }
            LayoutRebuilder.ForceRebuildLayoutImmediate(transform as RectTransform);
        }
        
        public void ReplaceRecipes(List<RecipeData> recipes)
        {
            ClearCraftableItems();
            AddRecipes(recipes);
        }

        public void CreateCraftableItemUI(RecipeData recipeData)
        {
            if (craftableItemUIs.ContainsKey(recipeData.Output.Id)) /// disregard dupes
            {
                return;
            }

            var craftableUI = Game.UI.Create<CraftableItemUI>("Craftable Item", craftablesHolder);
            craftableUI.SetRecipe(recipeData);
            craftableUI.OnClick += OnClickCraftable;

            UpdateCraftableStatusForPlayer(craftableUI);

            craftableItemUIs[recipeData.Output.Id] = craftableUI;
        }

        void UpdateCraftableStatusForPlayer(CraftableItemUI craftable)
        {
            if (PlayerHasMaterialsFor(craftable.RecipeData))
            {
                craftable.Status = CraftableItemStatus.Craftable;
            }
            else
            {
                craftable.Status = CraftableItemStatus.Uncraftable;
            }
        }

        bool PlayerHasMaterialsFor(RecipeData data)
        {
            foreach (var material in data.Materials)
            {
                if (!player.Inventory.Bag.IdItemCount.TryGetValue(material.Id, out int count)) return false;
                if (count < material.Count) return false;
            }

            return true;
        }

        void ClearCraftableItems()
        {
            foreach (var craftable in craftableItemUIs.Values)
            {
                craftable.Destroy();
            }
            craftableItemUIs.Clear();
        }

        #endregion

        
        #region Materials list

        public void DisplayMaterials(RecipeData data)
        {
            ClearMaterialSlots();

            foreach (var material in data.Materials)
            {
                CreateMaterialSlot(material);
            }
        }

        void CreateMaterialSlot(Item material)
        {
            var matSlot = Game.UI.Create<MaterialSlotUI>("Material Slot", materialSlotsHolder);
            matSlot.SetDisplayedItem(material);
            matSlot.Needed = material.Count;

            /// Retrieve cached count for Item of Id
            if (player.Inventory.Bag.IdItemCount.TryGetValue(material.Id, out var count))
            {
                /// Can also change colors here if you want
                matSlot.Present = count;
            }

            materialSlotUIs.Add(matSlot);
        }        

        void ClearMaterialSlots()
        {
            foreach (var slot in materialSlotUIs)
            {
                Destroy(slot.gameObject);
            }
            materialSlotUIs.Clear();
        }

        public void InitializeCrafter(Crafter crafter)
        {
            Unsubscribe();
            
            crafter.OnCraftStart += OnCraftStart;
        }

        
        #region Crafter event callbacks

        void OnCraftStart(object sender, CraftingRoutine routine)
        {
            
        }

        #endregion


        void Unsubscribe()
        {
            if (crafter == null) return;



        }

        #endregion
    }
}