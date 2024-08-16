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
        Container FuelContainer;

        protected bool IsFuelRemainingAvailable()
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

        public void OnFuelCheck(object sender, EventArgs e)
        {
            var _craftingRoutineInstance = (CraftingRoutine)sender;
            
            if (!IsFuelRemainingAvailable())
            {
                // foreach(CraftingRoutine routine in _craftingRoutineInstance.RoutineList)
                // {
                //     StopCoroutine(routine.CraftCoroutine());
                //     // routine._secondsElapsed = 0; /// seconds "elapsed" is not something to set
                // }
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