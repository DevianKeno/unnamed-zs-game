using System;
using System.Collections.Generic;

using UnityEngine;

using UZSG.Systems;
using UZSG.Entities;
using UZSG.Inventory;
using UZSG.Items;
using UZSG.UI;
using UZSG.Data;

namespace UZSG.Crafting
{
    /// <summary>
    /// Base abstract class for all crafting logic.
    /// </summary>
    public abstract class Crafter : MonoBehaviour
    {
        #region Events

        /// <summary>
        /// Called when the crafter start crafting an Item.
        /// </summary>
        public event EventHandler<CraftingRoutine> OnCraftStart;
        /// <summary>
        /// Called every second while the crafter crafts an Item.
        /// </summary>
        public event EventHandler<CraftingRoutine> OnCraftSecondsss;
        /// <summary>
        /// Called when the crafter finishes crafting an Item.
        /// </summary>
        public event EventHandler<CraftingRoutine> OnCraftFinished;

        #endregion


        protected bool CheckMaterialAvailability(RecipeData recipe, Container input)
        {
            foreach (Item material in recipe.Materials)
            {
                var materialSlots = new List<ItemSlot>();
                int totalItemCount = 0;

                var tempSlots = new List<ItemSlot>();
                totalItemCount += input.CountItem(material, out var slots);
                materialSlots.AddRange(tempSlots);

                if (totalItemCount < material.Count)
                {
                    print("Materials required does not match the current container");
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Consumes items from the container
        /// </summary>
        protected virtual List<Item> TakeItems(RecipeData recipe, Container input)
        {
            var materialSet = new List<Item>();

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
                        materialSet.Add(_takenItem);
                    }
                    else
                    {
                        remainingCount -= slot.Item.Count;
                        Item _takenItem = slot.TakeAll();
                        materialSet.Add(_takenItem);
                    }
                }
            }

            return materialSet;
        }
        
        /// <summary>
        /// Gives player the materials based on the RecipeData. It does not check if the player already takes the item so use checking logic to avoid situations such as item cloning
        /// </summary>
        protected virtual bool CanReturnItems(Container input, List<Item> materialSet)
        {
            return input.CanPutItems(materialSet);
        }

        protected void MigrateOutputToInput(Container input, Container output) //use only for debugging purposes
        {
            foreach (ItemSlot slot in output.Slots)
            {
                input.TryPutNearest(slot.TakeAll());
            }
        }


        //EVENTS SECTION
        protected void OnCraftSeconds(object sender, int secondsElapsed)
        {
            var _craftingInstanceSender = (CraftingRoutine) sender;
            print($"Crafting: {_craftingInstanceSender.recipeData.Id} - {_craftingInstanceSender.TimeRemaining} seconds Remaining");
        }

        protected void OnCraftFinish(object sender, CraftFinishedInfo unixTime)
        {
            var _craftingInstanceSender = (CraftingRoutine) sender;
            _craftingInstanceSender.output.TryPutNearest(new Item(_craftingInstanceSender.recipeData.Output));
            _craftingInstanceSender.routineList.Remove(_craftingInstanceSender);
        }
    }
}