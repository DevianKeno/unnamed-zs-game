using System;
using System.Collections;
using UnityEngine;
using UZSG.Data;
using UZSG.Inventory;

namespace UZSG.Crafting 
{
    public enum CraftingRoutineStatus {
        Prepared, Started, Ongoing, CraftSingle, Finished, Canceled
    }

    public class CraftingRoutine
    {
        protected RecipeData recipe;
        public RecipeData Recipe => recipe;
        public CraftingRoutineStatus Status { get; set; }
        public DateTime StartTime { get; set;}
        public DateTime EndTime { get; set;}
        
        /// <summary>
        /// Seconds elapsed for the entire Crafting Routine.
        /// </summary>
        public float SecondsElapsed { get; protected set; }
        /// <summary>
        /// Seconds left for the entire Crafting Routine.
        /// </summary>
        public float SecondsLeft => (int) Recipe.DurationSeconds * TotalYield - SecondsElapsed;
        /// <summary>
        /// Progress of the entire Crafting Routine. [0-1]
        /// </summary>
        public float Progress =>  (float)SecondsElapsed / (recipe.DurationSeconds * TotalYield);

        /// <summary>
        /// Seconds elapsed for one single craft in this Crafting Routine.
        /// </summary>
        public float SecondsElapsedSingle { get; protected set; }
        /// <summary>
        /// Seconds left for one single craft in this Crafting Routine.
        /// </summary>
        public float SecondsLeftSingle => (int) Recipe.DurationSeconds - SecondsElapsedSingle;
        /// <summary>
        /// Progress of a single crafted Item. [0-1]
        /// </summary>
        public float ProgressSingle => (float)SecondsElapsedSingle / recipe.DurationSeconds;
        
        public int CurrentYield { get; protected set; } 
        public int RemainingYield { get; protected set; }
        public int TotalYield { get; protected set; }
        protected CraftItemOptions options;
        public CraftItemOptions Options => options;
        protected ItemSlot outputSlot;
        public bool IsFueled { get; internal set; } 

        #region Events
        
        public event Action<CraftingRoutine> OnNotify;
        public event EventHandler<float> OnCraftSecond;
        public event EventHandler OnFuelCheck;

        #endregion


        public CraftingRoutine(CraftingRoutineOptions conf, bool isUsingFuel)
        {
            // this.Recipe = conf.Recipe;
            // // this.MaterialSets = conf.MaterialSets;
            // this.output = conf.Output;
            // this.RoutineList = conf.RoutineList;
        }
        
        public CraftingRoutine(CraftItemOptions options)
        {
            this.options = options;

            recipe = options.Recipe;
            TotalYield = options.Count;
            outputSlot = options.OutputSlot;
            IsFueled = options.IsFuel;
        }

        public void Prepare()
        {
            Status = CraftingRoutineStatus.Prepared;
            OnNotify?.Invoke(this);
        }

        public IEnumerator StartCraftCoroutine()
        {
            Status = CraftingRoutineStatus.Started;
            StartTime = DateTime.Now;
            OnNotify?.Invoke(this);

            SecondsElapsed = 0;
            CurrentYield = 0;
            RemainingYield = TotalYield;

            for (int i = 0; i < TotalYield; i++)
            {
                Status = CraftingRoutineStatus.Ongoing;
                SecondsElapsedSingle = 0;

                while (SecondsElapsedSingle < Recipe.DurationSeconds)
                {
                    if (IsFueled)
                    {
                        OnFuelCheck?.Invoke(this, EventArgs.Empty);
                    }

                    yield return new WaitForSeconds(0.1f);
                    SecondsElapsedSingle += 0.1f;
                    SecondsElapsed += 0.1f;
                    OnCraftSecond?.Invoke(this, SecondsLeftSingle);
                    OnNotify?.Invoke(this);
                }

                CurrentYield++;
                RemainingYield--;
                Status = CraftingRoutineStatus.CraftSingle;
                OnNotify?.Invoke(this);
            }

            Status = CraftingRoutineStatus.Finished;
            EndTime = DateTime.Now;
            OnNotify?.Invoke(this);
        }

        public void Cancel()
        {
            Status = CraftingRoutineStatus.Canceled;
            OnNotify?.Invoke(this);
        }
    }
}
