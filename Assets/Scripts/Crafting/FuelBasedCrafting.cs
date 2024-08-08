using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UZSG.Items;

namespace UZSG.Crafting
{
    public class FuelBasedCrafting : Crafter
    {
        public float FuelRemaining = 0;
        Container FuelContainer;

        protected bool isFuelRemainingAvailable() {
            if (FuelRemaining > 0) {
                return true;
            }
            return false;
        }

        protected bool isFuelAvailable() {
            if (FuelContainer.Slots[0].IsEmpty){
                return false;
            }
            return true;
        }

        public void ConsumeFuel()
        {
            if (isFuelAvailable() || isFuelRemainingAvailable()) return;
            FuelContainer.TakeItems(0, 1);
            StartCoroutine(BurnFuel());
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