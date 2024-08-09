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

        protected bool isFuelRemainingAvailable() {
            return FuelRemaining > 0;
        }

        protected bool isFuelAvailable() {
            if (FuelContainer.Slots[0].IsEmpty){
                return false;
            }
            return true;
        }

        public void ConsumeFuel()
        {
            if (isFuelRemainingAvailable()) return;
            if (!isFuelAvailable()) return;
            FuelContainer.TakeItems(0, 1);
            StartCoroutine(BurnFuel());
        }

        public void OnFuelCheck(object sender, EventArgs e){
            var _craftingRoutineInstance = (CraftingRoutine)sender;
            if (!isFuelRemainingAvailable()){
                foreach(CraftingRoutine routine in _craftingRoutineInstance.routineList){
                    StopCoroutine(routine.CraftCoroutine());
                    routine.secondsElapsed = 0;
                }
            }
        }

        IEnumerator BurnFuel(){
            while (isFuelRemainingAvailable())
            {
                yield return new WaitForSeconds(1);
                FuelRemaining -= 1;
                if(!isFuelRemainingAvailable())
                {
                    ConsumeFuel();
                }
            }
        }
    }
}