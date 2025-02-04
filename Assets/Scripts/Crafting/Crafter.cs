using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using MEC;

using UZSG.Inventory;
using UZSG.Items;
using UZSG.Data;
using UZSG.Objects;
using UZSG.Entities;

namespace UZSG.Crafting
{
    /// <summary>
    /// Base abstract class for all crafting logic.
    /// </summary>
    public abstract class Crafter : MonoBehaviour
    {
        protected Workstation workstation;
        
        protected int maxSimultaneousCrafts = 1;
        protected int availableCraftSlots;
        protected bool simultaneousCrafting = false;
        protected bool autoCraftNext = true;

        protected List<CraftingRoutine> routines = new();
        public List<CraftingRoutine> Routines => routines;

        #region Events

        /// <summary>
        /// Called everytime the crafting routine notifies.
        /// </summary>C
        public event Action<CraftingRoutine> OnRoutineNotify;
        /// <summary>
        /// Called every second while the crafter crafts an Item.
        /// <c>int</c> is total time elapsed.
        /// </summary>
        public event Action<CraftingRoutine, float> OnRoutineSecond;

        #endregion


        internal void Initialize(Workstation w)
        {
            this.workstation = w;
            availableCraftSlots = maxSimultaneousCrafts;
        }

        public virtual void CraftNewItem(ref CraftItemOptions options, bool begin = true)
        {
            var routine = new CraftingRoutine(options);
            
            routine.OnNotify += OnRoutineEventCall;
            routine.OnCraftSecond += OnCraftSecond;

            routines.Add(routine);
            routine.Prepare();
            if (begin && availableCraftSlots > 0)
            {
                CraftNextAvailable();
            }
        }

        public void CraftNextAvailable()
        {
            if (!routines.Any()) return;

            var nextRoutine = GetNextRoutine();
            if (nextRoutine != null)
            {
                StartCoroutine(nextRoutine.StartCraftCoroutine());
                availableCraftSlots--;
            }
        }

        public void ContinueRemainingCraft()
        {
            if (!routines.Any()) return;

            var pausedRoutine = GetPausedRoutine();
            if (pausedRoutine != null)
            {
                StartCoroutine(pausedRoutine.StartCraftCoroutine());
            }
        }

        CraftingRoutine GetNextRoutine()
        {
            foreach (var r in routines)
            {
                if (r.Status == CraftingRoutineStatus.Prepared)
                {
                    return r;
                }
            }
            return null;
        }

        CraftingRoutine GetPausedRoutine()
        {
            foreach (var r in routines)
            {
                if (r.Status == CraftingRoutineStatus.Paused)
                {
                    return r;
                }
            }
            return null;
        }

        public void CancelIndex()
        {

        }

        public void PauseAll()
        {
            
        }


        #region Crafting routine event callbacks

        protected void OnRoutineEventCall(CraftingRoutine routine)
        {            
            if (routine.Status == CraftingRoutineStatus.Finished)
            {
                routines.Remove(routine);
                routine.OnNotify -= OnRoutineEventCall;
                routine.OnCraftSecond -= OnCraftSecond;
                availableCraftSlots++;

                if (autoCraftNext) CraftNextAvailable();
            }

            OnRoutineNotify?.Invoke(routine);
        }

        protected void OnCraftSecond(object sender, float timeElapsed)
        {
            OnRoutineSecond?.Invoke((CraftingRoutine) sender, timeElapsed);
        }

        #endregion

        
        // protected bool CheckMaterialAvailability(RecipeData recipe)
        // {
        //     foreach (Item material in recipe.Materials)
        //     {
        //         if (inputContainer.IdItemCount.TryGetValue(material.Id, out var count))
        //         {
        //             if (count < material.Count) return false; /// insufficient items
        //         }
        //         else /// item does not exist in the container
        //         {
        //             return false;
        //         }
        //     }

        //     return true;
        // }

        protected bool CheckMaterialAvailability(RecipeData recipe, Container input)
        {
            foreach (Item material in recipe.Materials)
            {
                var materialSlots = new List<ItemSlot>();
                int totalItemCount = 0;

                var tempSlots = new List<ItemSlot>();
                totalItemCount += input.CountItem(material);
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
                    if (!material.Is(slot.Item)) continue;

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
            print($"Crafting: {_craftingInstanceSender.Recipe.Id} - {_craftingInstanceSender.SecondsLeft} seconds Remaining");
        }

        protected void OnCraftFinish(object sender)
        {
            var _craftingInstanceSender = (CraftingRoutine) sender;
            // _craftingInstanceSender.output.TryPutNearest(new Item(_craftingInstanceSender.Recipe.Output));
            // _craftingInstanceSender.RoutineList.Remove(_craftingInstanceSender);
        }
    }
}