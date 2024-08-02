using System.Collections.Generic;
using UnityEngine;
using UZSG.Crafting;
using UZSG.Items;
using UZSG.Systems;

namespace UZSG.UI
{
    public class CraftingGUI : MonoBehaviour
    {




        [SerializeField] RectTransform craftablesHolder;
        [SerializeField] RectTransform materialsHolder;

        public void InitializeRecipes()
        {
            List<RecipeData> recipes = new();

            foreach (var recipe in recipes)
            {
                AddCraftableItem(recipe);
            }
        }

        public void AddCraftableItem(RecipeData data)
        {
            var outputItem = data.Output;
            var ui = Game.UI.Create<CraftableItemUI>("Craftable Item", craftablesHolder);
            ui.SetItem(outputItem.Data);
        }
    }
}