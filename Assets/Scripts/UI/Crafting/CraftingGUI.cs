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

namespace UZSG.UI
{
    public class CraftingGUI : WorkstationGUI
    {
        RecipeData _selectedRecipe;
        List<CraftableItemUI> craftablesUI = new();
        List<ItemSlotUI> materialSlots = new();

        [SerializeField] Crafter crafter;


        [SerializeField] CraftedItemDisplayUI craftedItemDisplay;
        [SerializeField] RectTransform craftablesHolder;
        [SerializeField] RectTransform materialsHolder;
        [SerializeField] Button craftButton;
        [SerializeField] Button closeButton;

        internal void BindCrafter(Crafter crafter)
        {
            this.crafter = crafter;
        }

        void OnClickCraftable(CraftableItemUI craftable)
        {
            craftedItemDisplay.SetDisplayedItem(craftable.RecipeData.Output);
            DisplayMaterials(craftable.RecipeData);
            LayoutRebuilder.ForceRebuildLayoutImmediate(transform as RectTransform);
        }

        public void ResetDisplayed()
        {
            craftedItemDisplay.ResetDisplayed();
            ClearCraftables();
            ClearMaterials();
        }

        public override void OnShow()
        {
            ResetDisplayed();

            craftButton.onClick.AddListener(() =>
            {
                CraftItem();
            });
            closeButton.onClick.AddListener(() =>
            {
                Hide();
            });
        }

        void CraftItem()
        {
            if (crafter.TryCraftRecipe(_selectedRecipe))
            {
                
            }
        }


        #region Craftables list

        public void AddRecipes(List<RecipeData> recipes)
        {
            foreach (var recipe in recipes)
            {
                AddCraftableItem(recipe);
            }
            LayoutRebuilder.ForceRebuildLayoutImmediate(transform as RectTransform);
        }

        public void ReplaceRecipes(List<RecipeData> recipes)
        {
            ClearCraftables();
            AddRecipes(recipes);
        }

        public void AddCraftableItem(RecipeData data)
        {
            var craftableUI = Game.UI.Create<CraftableItemUI>("Craftable Item", craftablesHolder);
            craftableUI.SetRecipe(data);
            craftableUI.OnClick += OnClickCraftable;
            craftablesUI.Add(craftableUI);
        }

        void ClearCraftables()
        {
            foreach (var craftable in craftablesUI)
            {
                craftable.Destroy();
            }
            craftablesUI.Clear();
        }

        #endregion

        
        #region Materials list

        public void DisplayMaterials(RecipeData data)
        {
            ClearMaterials();
            foreach (var item in data.Materials)
            {
                AddMaterial(item);
            }
        }

        void AddMaterial(Item item)
        {
            var slot = Game.UI.Create<ItemSlotUI>("Item Slot", materialsHolder);
            slot.SetDisplayedItem(item);
            materialSlots.Add(slot);
        }

        void ClearMaterials()
        {
            foreach (var slotUI in materialSlots)
            {
                slotUI.Destroy();
            }
            materialSlots.Clear();
        }

        #endregion
    }
}