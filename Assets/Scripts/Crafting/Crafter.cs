using System.Collections.Generic;
using UnityEngine;

using UZSG.Systems;
using UZSG.Entities;
using UZSG.Inventory;
using UZSG.Items;
using UZSG.UI;
using UZSG.Data;
using System.Collections;
using System;

namespace UZSG.Crafting
{
    public abstract class Crafter : MonoBehaviour
    {
        public List<Container> containers;

        [SerializeField] CraftingGUI _GUI;

        // public void ViewRecipe(Item item)
        // {
        //     RecipeData recipes = Game.Recipes.GetRecipeData(item.Id);

        //     foreach (Item material in recipes.Materials)
        //     {
        //         print(material.Name);
        //     }
        // }

        public void AddRecipes(List<RecipeData> recipes)
        {
            _GUI.AddRecipes(recipes);
        }
        
        public void BindUI(CraftingGUI gui)
        {
            _GUI = gui;
            _GUI.BindCrafter(this);
        }

        public void AddContainer(Container container)
        {
            containers.Add(container);
        }

        /// <summary>
        /// Consumes items in the container and returns an item whenever the required resource is available. 
        /// Returns a dictionary of slots pertaining to the recipe. 
        /// </summary>
        protected bool CheckMaterialAvailability(Item item, int recipeIndex, Dictionary<Item, List<ItemSlot>> dictSlots)
        {
            if (recipeIndex > item.Data.Recipes.Count)
            {
                print("Recipe index out of bound");
                return false;
            }

            foreach (Item material in item.Data.Recipes[recipeIndex].Materials)
            {
                var materialSlots = new List<ItemSlot>();
                int totalItemCount = 0;

                foreach (Container container in containers)
                {
                    var tempSlots = new List<ItemSlot>();
                    totalItemCount += container.CountItem(material, out tempSlots);
                    materialSlots.AddRange(tempSlots);
                }

                if (totalItemCount < material.Count){
                    print("Materials required does not match the current container");
                    return false;
                } 

                dictSlots.Add(material, materialSlots);
            }

            return true;
        }

        /// <summary>
        /// Consumes items from the container
        /// </summary>
        protected virtual void TakeItems(Item item, int recipeIndex, Dictionary<Item, List<ItemSlot>> dictSlots)
        {
            foreach (Item material in item.Data.Recipes[recipeIndex].Materials)
            {
                int remainingCount = material.Count;

                foreach (ItemSlot slot in dictSlots[material])
                {
                    /*
                        comparator checks the difference of the remaining count and the
                        required count of the material.
                    */                    
                    if (remainingCount <= 0)
                    {
                        break;
                    }

                    int comparator = slot.Item.Count - remainingCount;

                    if (comparator > 0)
                    {
                        remainingCount -= material.Count;
                        slot.TakeItems(material.Count);
                    }
                    else 
                    {
                        remainingCount -= slot.Item.Count;
                        slot.TakeAll();
                    }
                }
            }
        }

        /// <summary>
        /// Consumes items in the container and returns an item whenever the required resource is available
        /// </summary>
        public virtual void CraftItem(Item item, int recipeIndex)
        {
            var dictSlots = new Dictionary<Item, List<ItemSlot>>();

            if (!CheckMaterialAvailability(item, recipeIndex, dictSlots))
            {
                return;
            }

            TakeItems(item, recipeIndex, dictSlots);

            foreach (Container container in containers)
            {
                if (container.TryPutNearest(new Item(item))) break;
            }
        }

        public bool TryCraftRecipe(RecipeData recipe)
        {
            return false;
        }
    }
}