using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
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
        public Action OnFuelUpdate;

        public Action<float> OnFuelReload;

        private float FuelConsumptionRate = 0.1f;

        private bool _mustContinue = false;

        public bool IsFuelRemainingAvailable()
        {
            return FuelRemaining > 0;
        }

        public bool IsFuelAvailable()
        {
            if (FuelContainer.Slots[0].IsEmpty)
            {
                return false;
            }
            return true;
        }

        public void StartBurn()
        {
            StartCoroutine(BurnFuel());
        }

        public bool TryConsumeFuel()
        {
            if (!IsFuelAvailable()) return false;
            var fuel = FuelContainer.TakeFrom(0, 1);
            FuelRemaining = fuel.Data.FuelDuration;
            OnFuelReload?.Invoke(fuel.Data.FuelDuration);
            OnFuelUpdate?.Invoke();
            return true;
        }

        public override void CraftNewItem(ref CraftItemOptions options, bool begin = true)
        {
            var routine = new CraftingRoutine(options);
            
            routine.OnNotify += OnRoutineEventCall;
            routine.OnCraftSecond += OnCraftSecond;
            routine.OnFuelCheck += OnFuelCheck;
            routine.IsFueled = true;

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

        public void OnBurnStop(object sender, EventArgs e)
        {

        }


        IEnumerator BurnFuel()
        {
            while (IsFuelRemainingAvailable())
            {
                yield return new WaitForSeconds(FuelConsumptionRate);
                FuelRemaining -= FuelConsumptionRate;
                OnFuelUpdate?.Invoke();
                print("Fuel Remaing: " + FuelRemaining);
                if(!IsFuelRemainingAvailable())
                {
                    if (routines.Count <= 0) yield break;
                    if(TryConsumeFuel())
                    {
                        print("Fuel Consumed");
                    }
                }
            }
        }
    }
}