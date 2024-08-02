using System.Collections.Generic;
using UnityEngine;
using UZSG.Entities;
using UZSG.Inventory;
using UZSG.Items;
using UZSG.Systems;

namespace UZSG.Crafting
{
    public abstract class Crafter : MonoBehaviour
    {
        public List<Container> containers;

        // public void ViewRecipe(Item item)
        // {
        //     RecipeData recipes = Game.Recipes.GetRecipeData(item.Id);

        //     foreach (Item material in recipes.Materials)
        //     {
        //         print(material.Name);
        //     }
        // }

        /// <summary>
        /// Consumes items in the container and returns an item whenever the required resource is available. 
        /// Returns a dictionary of slots pertaining to the recipe. 
        /// </summary>
        protected bool CheckMaterialAvailability(Item item, int recipeIndex, out Dictionary<Item, List<ItemSlot>> dictSlots){
            dictSlots = null;
            
            if (recipeIndex > item.Data.Recipes.Count){
                print("Recipe index out of bound");
                return false;
            }

            foreach (Item material in item.Data.Recipes[recipeIndex].Materials)
            {

                foreach (Container container in containers)
                {
                    var materialSlots = new List<ItemSlot>();

                    if (container.ContainsCount(item: material, material.Count, out materialSlots))
                    {
                        dictSlots.Add(material, materialSlots);
                    } else {
                        print("Materials Required does not match players current Inventory");
                        return false;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Consumes items from the container
        /// </summary>
        protected virtual void TakeItems(Item item, int recipeIndex, Dictionary<Item, List<ItemSlot>> dictSlots){

            foreach (Item material in item.Data.Recipes[recipeIndex].Materials)
            {
                int remainingCount = material.Count;

                foreach (ItemSlot slot in dictSlots[material])
                {
                    /*
                        comparator checks the difference of the remaining count and the
                        required count of the material.
                    */
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

                    if (remainingCount <= 0)
                    {
                        continue;
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
            if ( !CheckMaterialAvailability(item, recipeIndex, out dictSlots)) {
                return;
            }

            TakeItems(item, recipeIndex, dictSlots);

            foreach (Container container in containers){
                if (container.TryPutNearest(new Item(item))) break;
            }
            
        }
    }
}