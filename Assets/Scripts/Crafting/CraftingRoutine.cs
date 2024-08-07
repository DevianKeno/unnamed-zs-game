using System;
using System.Collections;
using UnityEngine;
using UZSG.Data;
using UZSG.Items;

namespace UZSG.Crafting 
{
    public class CraftingRoutine
    {
        public event EventHandler<int> OnCraftSecond;
        public event EventHandler<CraftFinishedInfo> OnCraftFinish;
        public RecipeData RecipeData;

        int secondsElapsed = 0;
        

        public CraftingRoutine(RecipeData recipeData)
        {
            this.RecipeData = recipeData;
        }


        public int GetTimeRemaining(){
            return (int)RecipeData.DurationSeconds - secondsElapsed;
        }


        public IEnumerator CraftCoroutine()
        {
            CraftFinishedInfo TimeInfo;

            TimeInfo.StartTime = DateTime.Now;
            while (secondsElapsed < RecipeData.DurationSeconds)
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

