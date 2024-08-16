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
        public DateTime StartTime { get; set ;}
        public DateTime EndTime { get; set ;}
        // public List<Item> MaterialSets;
        // public List<CraftingRoutine> RoutineList = new();
        // public Container output;
        public int SecondsElapsed { get; protected set; }
        public int SecondsLeft => (int) Recipe.DurationSeconds - SecondsElapsed;
        /// <summary>
        /// 0-1
        /// </summary>
        public float Progress =>  SecondsElapsed / recipe.DurationSeconds;
        public int CurrentYield { get; protected set; } 
        public int RemainingYield { get; protected set; }
        public int TotalYield { get; protected set; }
        protected CraftItemOptions options;
        public CraftItemOptions Options => options;
        protected ItemSlot outputSlot;
        public bool IsFueled { get; internal set; } 


        #region Events
        
        public event Action<CraftingRoutine> OnNotify;
        /// <summary>
        /// <c>int</c> is time remaining. /// pls change into formattable (4:20, 55)
        /// </summary>
        public event EventHandler<int> OnCraftSecond;
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
            for (int i = CurrentYield; i < TotalYield; i++)
            {
                Status = CraftingRoutineStatus.Ongoing;

                while (SecondsElapsed < Recipe.DurationSeconds)
                {
                    if (IsFueled)
                    {
                        OnFuelCheck?.Invoke(this, new());
                    }

                    yield return new WaitForSeconds(1f);
                    SecondsElapsed++;
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

