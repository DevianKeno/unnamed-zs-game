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
        /// <summary>
        /// Consumes items in the container and returns an item whenever the required resource is available. 
        /// Returns a dictionary of slots pertaining to the recipe. 
        /// </summary>
        /// 



        /// <summary>
        /// Consumes items in the container and returns an item whenever the required resource is available. 
        /// Returns a dictionary of slots pertaining to the recipe. 
        /// </summary>
        ///
        //Function for checking the availability of materials using RecipeData
        protected bool CheckMaterialAvailability(RecipeData recipe, Dictionary<Item, List<ItemSlot>> dictSlots){

            foreach (Item material in recipe.Materials)
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


        //Function for checking the availability of materials within a specific Container using RecipeData
        protected bool CheckMaterialAvailabilityWithinContainer(RecipeData recipe, int containerIndex, Dictionary<Item, List<ItemSlot>> dictSlots){
            foreach (Item material in recipe.Materials)
            {
                var materialSlots = new List<ItemSlot>();
                
                int _totalItemCount = 0;

                var tempSlots = new List<ItemSlot>();
                _totalItemCount += containers[containerIndex].CountItem(material, out tempSlots);
                materialSlots.AddRange(tempSlots);

                if (_totalItemCount < material.Count){
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
        protected virtual void TakeItems(RecipeData recipe, Dictionary<Item, List<ItemSlot>> dictSlots){

            foreach (Item material in recipe.Materials)
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
        public virtual void CraftItem(RecipeData recipe)
        {
            var dictSlots = new Dictionary<Item, List<ItemSlot>>();
            if ( !CheckMaterialAvailability(recipe, dictSlots)) {
                return;
            }

            TakeItems(recipe, dictSlots);

            foreach (Container container in containers){
                if (container.TryPutNearest(new Item(recipe.Output))) break;
            }
        }

        /// <summary>
        /// Consumes items in a specific container and returns an item whenever the required resource is available within that container
        /// </summary>
        public virtual void ContainerCraftItem(RecipeData recipe, int containerIndex)
        {
            var dictSlots = new Dictionary<Item, List<ItemSlot>>();
            if ( !CheckMaterialAvailabilityWithinContainer(recipe, containerIndex, dictSlots)) {
                return;
            }

            TakeItems(recipe, dictSlots);

            foreach (Container container in containers){
                if (container.TryPutNearest(new Item(recipe.Output))) break;
            }
        }

    }
}