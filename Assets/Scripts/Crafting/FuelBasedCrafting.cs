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

        //NOTE: DO NOT MODIFY THESE TWO VARIABLES BELOW UNLESS NECESSARY
        private float FuelConsumptionUpdateRate = 0.1f;
        private int FuelConsumptionRate = 100;
        


        private bool _mustContinue = false;


        //Checks if there is a consumed fuel remaining in the crafting instance
        public bool IsFuelRemainingAvailable()
        {
            return FuelRemaining > 0;
        }

        //checks if the fuel slot has fuel in it
        public bool IsFuelAvailable()
        {
            if (FuelContainer.Slots[0].IsEmpty)
            {
                return false;
            }
            return true;
        }


        //Burns consumed fuel
        public void StartBurn()
        {
            StartCoroutine(BurnFuel());
        }


        //Tries to consume fuel if available
        public bool TryConsumeFuel()
        {
            if (!IsFuelAvailable()) return false;
            var fuel = FuelContainer.TakeFrom(0, 1);
            FuelRemaining = fuel.Data.FuelDuration * 1000;
            OnFuelReload?.Invoke(fuel.Data.FuelDuration);
            OnFuelUpdate?.Invoke();
            return true;
        }

        //an override function of CraftNewItem specifically for fuel based crafting
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

        //An event function called whenever a crafting routine request the status of fuel in the crafting
        public void OnFuelCheck(object sender, EventArgs e)
        {
            var _craftingRoutineInstance = (CraftingRoutine)sender;
            
            if (!IsFuelRemainingAvailable())
            {
                _craftingRoutineInstance.StopCrafting = true;
            }
        }


        //A couroutine responsible for the burning of consumed fuel inside the crafting instance
        IEnumerator BurnFuel()
        {
            while (IsFuelRemainingAvailable())
            {
                yield return new WaitForSeconds(FuelConsumptionUpdateRate);
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