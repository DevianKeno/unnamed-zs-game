using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using UZSG.Data;
using UZSG.Items;

namespace UZSG.Crafting 
{
    public class CraftingRoutine
    {
        public RecipeData recipeData;
        public List<Item> materialSets;
        public List<CraftingRoutine> routineList;
        public Container output;
        public int secondsElapsed = 0;
        public bool isUsingFuel = false;


        #region Events
        
        public event EventHandler<int> OnCraftSecond;
        public event EventHandler<CraftFinishedInfo> OnCraftFinish;
        public event EventHandler OnFuelCheck;
        public event EventHandler OnCraftStop;

        #endregion


        public CraftingRoutine(CraftingRoutineOptions conf, bool isUsingFuel){
            this.recipeData = conf.Recipe;
            this.materialSets = conf.MaterialSets;
            this.output = conf.Output;
            this.routineList = conf.RoutineList;
        }
        public CraftingRoutine(CraftingRoutineOptions conf){
            this.recipeData = conf.Recipe;
            this.materialSets = conf.MaterialSets;
            this.output = conf.Output;
            this.routineList = conf.RoutineList;
        }

        public int TimeRemaining
        {
            get => (int) recipeData.DurationSeconds - secondsElapsed;
        }

        public IEnumerator CraftCoroutine()
        {
            var TimeInfo = new CraftFinishedInfo()
            {
                StartTime = DateTime.Now
            };

            while (secondsElapsed < recipeData.DurationSeconds)
            {
                if (isUsingFuel)
                {
                    OnFuelCheck?.Invoke(this, EventArgs.Empty);
                }
                yield return new WaitForSeconds(1f);
                secondsElapsed++;
                OnCraftSecond?.Invoke(this, TimeRemaining);
            }

            TimeInfo.EndTime = DateTime.Now;
            OnCraftFinish?.Invoke(this, TimeInfo);
            yield break;
        }
    }
}

