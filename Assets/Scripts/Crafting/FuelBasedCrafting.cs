using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UZSG.Items;

namespace UZSG.Crafting
{
    public class FuelBasedCrafting : Crafter
    {
        public Container InputContainer;
        public float FuelRemaining = 0;
        public Container FuelContainer;

        public bool IsFuelRemainingAvailable()
        {
            return FuelRemaining > 0;
        }

        protected bool IsFuelAvailable()
        {
            if (FuelContainer.Slots[0].IsEmpty)
            {
                return false;
            }
            return true;
        }

        public void ConsumeFuel()
        {
            if (IsFuelRemainingAvailable()) return;
            if (!IsFuelAvailable()) return;

            FuelContainer.TakeFrom(0, 1);
         
            StartCoroutine(BurnFuel());
        }

        public override void CraftNewItem(ref CraftItemOptions options, bool begin = true)
        {
            var routine = new CraftingRoutine(options);
            
            routine.OnNotify += OnRoutineEventCall;
            routine.OnCraftSecond += OnCraftSecond;
            routine.OnFuelCheck += OnFuelCheck;

            routines.Add(routine);
            routine.Prepare();
            if (begin && availableCraftSlots > 0)
            {
                CraftNextAvailable();
            }
        }


        public void OnFuelCheck(object sender, EventArgs e)
        {
            var _craftingRoutineInstance = (CraftingRoutine)sender;
            
            if (!IsFuelRemainingAvailable())
            {
                _craftingRoutineInstance.StopCrafting = true;
            }
        }


        IEnumerator BurnFuel()
        {
            while (IsFuelRemainingAvailable())
            {
                yield return new WaitForSeconds(1);
                FuelRemaining -= 1;
                if(!IsFuelRemainingAvailable())
                {
                    ConsumeFuel();
                }
            }
        }
    }
}