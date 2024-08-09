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
        public event EventHandler<int> OnCraftSecond;
        public event EventHandler<CraftFinishedInfo> OnCraftFinish;
        public event EventHandler OnFuelCheck;
        public event EventHandler OnCraftStop;


        public RecipeData recipeData;
        public List<Item> materialSets;
        public List<CraftingRoutine> routineList;
        public Container output;
        public int secondsElapsed = 0;
        public bool isUsingFuel = false;

        public CraftingRoutine(CraftingRoutineOptions conf, bool isUsingFuel){
            this.recipeData = conf.recipe;
            this.materialSets = conf.materialSets;
            this.output = conf.output;
            this.routineList = conf.routineList;
        }
        public CraftingRoutine(CraftingRoutineOptions conf){
            this.recipeData = conf.recipe;
            this.materialSets = conf.materialSets;
            this.output = conf.output;
            this.routineList = conf.routineList;
        }

        public int GetTimeRemaining(){
            return (int)recipeData.DurationSeconds - secondsElapsed;
        }

        public IEnumerator CraftCoroutine()
        {
            CraftFinishedInfo TimeInfo;

            TimeInfo.StartTime = DateTime.Now;
            while (secondsElapsed < recipeData.DurationSeconds)
            {
                if(isUsingFuel)
                {
                    OnFuelCheck?.Invoke(this, EventArgs.Empty);
                }
                yield return new WaitForSeconds(1);
                secondsElapsed++;
                OnCraftSecond?.Invoke(this, GetTimeRemaining());
            }
            TimeInfo.EndTime = DateTime.Now;
            OnCraftFinish?.Invoke(this, TimeInfo);
            yield break;
        }
    }
}

