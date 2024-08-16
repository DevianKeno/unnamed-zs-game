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
        [Space]
        public float TimeSingle;
        public float TimeElapsedSingle;

        [Range(0, 1)]
        [SerializeField] protected float progressSingle;
        public float ProgressSingle
        {
            get
            {
                return progressSingle;
            }
            set
            {
                progressSingle = Mathf.Clamp(value, 0, 1);
                RefreshSingle();
            }
        }

        [SerializeField] protected ItemDisplayUI itemDisplayUI;
        [SerializeField] protected Image fillSingle;
        
        
        protected override void OnValidate()
        {
            base.OnValidate();

            RefreshSingle();
        }

        public void RefreshSingle()
        {
            fillSingle.fillAmount = ProgressSingle;
        }

        public void SetCraftingRoutine(CraftingRoutine routine)
        {
            itemDisplayUI.SetDisplayedItem(routine.Recipe.Output);
            TotalTime = routine.Recipe.DurationSeconds * routine.TotalYield;
            Progress = routine.Progress;
            ProgressSingle = routine.ProgressSingle;
        }
    }
}
