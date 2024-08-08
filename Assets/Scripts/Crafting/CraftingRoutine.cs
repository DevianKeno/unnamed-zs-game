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
        public RecipeData recipeData;
        public List<Item> materialSets;

        public Container output;
        int secondsElapsed = 0;

        public CraftingRoutine(RecipeData recipeData, List<Item> materialSets, Container output)
        {
            this.recipeData = recipeData;
            this.materialSets = materialSets;
            this.output = output;
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

