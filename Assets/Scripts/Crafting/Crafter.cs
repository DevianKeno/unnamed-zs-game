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
        public List<CraftingRoutine> craftingRoutineList = new();
        protected List<Container> InputContainers = new();
        public Container OutputContainer;

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

                foreach (Container container in InputContainers)
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
        protected bool CheckMaterialAvailabilityWithinContainer(RecipeData recipe, Container container, Dictionary<Item, List<ItemSlot>> dictSlots){
            foreach (Item material in recipe.Materials)
            {
                var materialSlots = new List<ItemSlot>();
                
                int _totalItemCount = 0;

                var tempSlots = new List<ItemSlot>();
                _totalItemCount += container.CountItem(material, out tempSlots);
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
        
        /// <summary>
        /// Gives player the materials based on the RecipeData. It does not check if the player already takes the item so use checking logic to avoid situations such as item cloning
        /// </summary>
        protected virtual void ReturnItems(RecipeData recipe){
            foreach (Container container in containers){
                foreach (Item item in recipe.Materials)
                    if (container.TryPutNearest(new Item(item))) break;
            }
        }
        /// <summary>
        /// Consumes items in the container and returns an item whenever the required resource is available
        /// </summary>

        public void CraftQueue(RecipeData recipe)
        {
            var dictSlots = new Dictionary<Item, List<ItemSlot>>();

            if ( !CheckMaterialAvailability(recipe, dictSlots))
            {
                return;
            }

            TakeItems(recipe, dictSlots);

            CraftingRoutine craftInstance = new CraftingRoutine(recipe);

            craftInstance.OnCraftFinish += OnCraftFinish;
            craftInstance.OnCraftSecond += OnCraftSeconds;

            craftingRoutineList.Add(craftInstance);

            StartCoroutine(craftInstance.CraftCoroutine());
        }

        protected void MigrateOutputToInput(Container input, Container output) //use only for debugging purposes
        {
            foreach (ItemSlot slot in output.Slots)
            {
                input.TryPutNearest(slot.TakeAll());
            }
        }
        private void OnCraftSeconds(object sender, int secondsElapsed)
        {
            var _craftingInstanceSender = (CraftingRoutine) sender;
            print($"Crafting: {_craftingInstanceSender.RecipeData.Id} - {_craftingInstanceSender.GetTimeRemaining()} seconds Remaining");
        }

        private void OnCraftFinish(object sender, CraftFinishedInfo unixTime)
        {
            var _craftingInstanceSender = (CraftingRoutine) sender;
            OutputContainer.TryPutNearest(new Item(_craftingInstanceSender.RecipeData.Output));
            MigrateOutputToInput(InputContainers[0], OutputContainer);
            craftingRoutineList.Remove(_craftingInstanceSender);
        }

        public void AddContainer(Container container)
        {
            if (container == null) return;

            containers.Add(container);
        }
    }
}