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

namespace UZSG.UI
{
    public class CraftingGUI : WorkstationGUI
    {
        List<CraftableItemUI> craftablesUI = new();
        List<ItemSlotUI> materialSlots = new();

        [SerializeField] Crafter Crafter;

        [SerializeField] TextMeshProUGUI titleText;
        [SerializeField] RectTransform craftablesHolder;
        [SerializeField] RectTransform materialsHolder;

        void OnClickCraftable(CraftableItemUI craftable)
        {
            DisplayMaterials(craftable.RecipeData);
            LayoutRebuilder.ForceRebuildLayoutImmediate(transform as RectTransform);
        }


        #region Craftables list

        public void InitializeCraftableRecipes(List<RecipeData> recipes)
        {
            ClearCraftables();
            foreach (var recipe in recipes)
            {
                AddCraftableItem(recipe);
            }
            LayoutRebuilder.ForceRebuildLayoutImmediate(transform as RectTransform);
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