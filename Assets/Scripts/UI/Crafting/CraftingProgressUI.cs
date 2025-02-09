using UnityEngine;
using UnityEngine.UI;
using TMPro;

using UZSG.Items;
using UZSG.Crafting;
using System;

namespace UZSG.UI
{
    public class CraftingProgressUI : RadialProgressUI
    {
        [SerializeField, Range(0, 1)] protected float progressSingle;
        public float ProgressSingle
        {
            get
            {
                return progressSingle;
            }
            set
            {
                progressSingle = Mathf.Clamp(value, 0f, 1f);
                RefreshSingle();
            }
        }
        CraftingRoutine routine;
        [SerializeField] protected ItemDisplayUI itemDisplayUI;
        [SerializeField] protected Image fillSingle;
        
        protected override void OnValidate()
        {
            base.OnValidate();
            RefreshSingle();
        }

        void Update()
        {
            if (!IsVisible) return;
                        
            Progress = routine.Progress;
            ProgressSingle = routine.ProgressSingle;
        }

        public void RefreshSingle()
        {
            fillSingle.fillAmount = ProgressSingle;
        }

        public void SetCraftingRoutine(CraftingRoutine routine)
        {
            this.routine = routine;
            itemDisplayUI.SetDisplayedItem(routine.RecipeData.Output);
            TotalTime = routine.RecipeData.CraftingTimeSeconds * routine.TotalYield;
            Progress = routine.Progress;
            ProgressSingle = routine.ProgressSingle;
        }
    }
}
