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
    public struct CraftFinishedInfo 
    {
            public DateTime StartTime;
            public DateTime EndTime;
    }
    public abstract class Crafter : MonoBehaviour
    {
        public List<CraftingRoutine> craftingRoutineList = new();
        // public Container InputContainer = new();
        // public Container OutputContainer;

        /// <summary>
        /// Consumes items in the container and returns an item whenever the required resource is available. 
        /// Returns a dictionary of slots pertaining to the recipe. 
        /// </summary>
        ///
        //Function for checking the availability of materials using RecipeData
        protected bool CheckMaterialAvailability(RecipeData recipe, Container input){

            foreach (Item material in recipe.Materials)
            {
                var materialSlots = new List<ItemSlot>();
                int totalItemCount = 0;

                var tempSlots = new List<ItemSlot>();
                totalItemCount += input.CountItem(material, out var slots);
                materialSlots.AddRange(tempSlots);


                if (totalItemCount < material.Count){
                    print("Materials required does not match the current container");
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Consumes items from the container
        /// </summary>
        protected virtual List<Item> TakeItems(RecipeData recipe, Container input){

            List<Item> _materialSet = new();

            foreach (Item material in recipe.Materials)
            {
                int remainingCount = material.Count;

                foreach (ItemSlot slot in input.Slots)
                {
                    if (remainingCount <= 0 ) break;
                    if (!material.CompareTo(slot.Item)) continue;

                    int comparator = slot.Item.Count - remainingCount;

                    if (comparator > 0)
                    {
                        remainingCount -= material.Count;
                        Item _takenItem = slot.TakeItems(material.Count);
                        _materialSet.Add(_takenItem);
                    }
                    else
                    {
                        remainingCount -= slot.Item.Count;
                        Item _takenItem = slot.TakeAll();
                        _materialSet.Add(_takenItem);
                    }
                }
            }

            return _materialSet;
        }
        
        /// <summary>
        /// Gives player the materials based on the RecipeData. It does not check if the player already takes the item so use checking logic to avoid situations such as item cloning
        /// </summary>
        protected virtual void ReturnItems(Container input, List<Item> materialSet)
        {
            foreach (Item item in materialSet)
            {
                input.TryPutNearest(item);
            }
        }

        /// <summary>
        /// Consumes items in the container and returns an item whenever the required resource is available
        /// </summary>
        public void CraftQueue(Container input, Container output, RecipeData recipe)
        {
            var dictSlots = new Dictionary<Item, List<ItemSlot>>();

            if ( !CheckMaterialAvailability(recipe, input))
            {
                return;
            }

            List<Item> _materialSets = TakeItems(recipe, input);

            CraftingRoutine craftInstance = new CraftingRoutine(recipe, _materialSets, output);

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
            print($"Crafting: {_craftingInstanceSender.recipeData.Id} - {_craftingInstanceSender.GetTimeRemaining()} seconds Remaining");
        }

        private void OnCraftFinish(object sender, CraftFinishedInfo unixTime)
        {
            var _craftingInstanceSender = (CraftingRoutine) sender;
            _craftingInstanceSender.output.TryPutNearest(new Item(_craftingInstanceSender.recipeData.Output));
            craftingRoutineList.Remove(_craftingInstanceSender);
        }

        // public void AddContainer(Container container, Container input)
        // {
        //     if (container == null) return;

        //     InputContainers.Add(container);
        // }
    }
}