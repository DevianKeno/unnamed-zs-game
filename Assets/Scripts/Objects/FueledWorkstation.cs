using System;
using System.Linq;

using UnityEngine;

using UZSG.Crafting;
using UZSG.Inventory;
using UZSG.Items;
using UZSG.UI;
using static UZSG.Crafting.CraftingRoutineStatus;

namespace UZSG.Objects
{
    /// <summary>
    /// Represents objects that can craft items but only when fueled.
    /// </summary>
    [RequireComponent(typeof(Crafter))]
    public class FueledWorkstation : CraftingStation
    {
        public Container FuelContainer { get; protected set; }
        public FueledCraftingGUI FueledCraftingGUI => (FueledCraftingGUI) base.GUI;

        float fuelRemainingSeconds = 0f;
        public float FuelRemainingSeconds => fuelRemainingSeconds;
        float lastFuelMaxDurationSeconds = 0f;
        /// <summary>
        /// The max fuel duration seconds of the last fuel consumed :)
        /// </summary>
        public float LastFuelMaxDuration => lastFuelMaxDurationSeconds;

        public bool CanFuelCraft
        {
            get => IsFueled || HasFuelItem;
        }
        /// <summary>
        /// Checks if there is a consumed fuel remaining in the crafting instance
        /// </summary>
        public bool IsFueled
        {
            get => fuelRemainingSeconds > 0;
        }
        /// <summary>
        /// Checks if the fuel slots still has fuel items in it.
        /// </summary>
        public bool HasFuelItem
        {
            get => FuelContainer.HasAny;
        }
        protected bool isCooking;
        public bool IsCooking => isCooking;
        
        /// <summary>
        /// Raised once whenever a fuel item is consumed.
        /// <c>float</c> is the fuel duration seconds of the fuel consumed.
        /// </summary>
        public event Action<float> OnConsumeFuel;
        /// <summary>
        /// Raised every frame when a fuel is being consumed.
        /// <c>float</c> is the fuel remaining seconds of the fuel being consumed.
        /// </summary>
        public event Action<float> OnFuelUpdate;
        /// <summary>
        /// Raised once when fuel completely runs out.
        /// </summary>
        public event Action OnFuelDepleted;

        protected override void OnPlace()
        {
            base.OnPlace();
            FuelContainer = new Container(WorkstationData.FuelSlotsSize);
            
            Crafter.OnRoutineNotify += OnCraftingRoutineNotify;
            FuelContainer.OnSlotItemChanged += OnFuelSlotChanged;
        }

        protected virtual void Update()
        {
            if (isCooking || WorkstationData.UsesFuelWhenIdle)
            {
                TryConsumeFuel(Time.deltaTime);
            }
        }

        public virtual bool TryFueledCraft(ref CraftItemOptions options)
        {
            if (!options.Recipe.RequiresFuel)
            {
                print("Your recipe does not require fuel to be crafted");
                return false;
            }
            if (!IsFueled && !HasFuelItem)
            {
                print("You cannot use this without any kind of fuel");
                return false;
            }
            if (Crafter.IsQueueFull)
            {
                return false;
            }

            var totalMaterials = CraftingUtils.CalculateTotalMaterials(options);
            if (!Player.Inventory.Bag.ContainsAll(totalMaterials))
            {
                if (GUI.IsVisible) CraftingUtils.PlayNoMaterialsSound();
                Game.Console.LogDebug($"Tried to craft '{options.Recipe.Output.Id}' but had insufficient materials.");
                return false;
            }

            _ = InputContainer.TakeItems(totalMaterials);
            Crafter.CraftNewItem(ref options);
            return true;
        }

        /// <summary>
        /// Raised for each milestone the crafting routine achieves lol. See <c>CraftingRoutineStatus</c> enum.
        /// </summary>
        protected virtual void OnCraftingRoutineNotify(CraftingRoutine routine)
        {
            switch (routine.Status)
            {
                case Started:
                {
                    isCooking = true;
                    break;
                }
                case Paused or Canceled:
                {
                    isCooking = false;
                    break;
                }
                case Finished:
                {
                    if (!Crafter.Routines.Any()) // there's nothing next in queue
                    {
                        isCooking = false;
                    }
                    break;
                }
            }
        }

        /// <summary>
        /// Raised when any of the fuel slots contents are changed.
        /// </summary>
        protected virtual void OnFuelSlotChanged(object sender, ItemSlot.ItemChangedContext e)
        {
            if (e.NewItem.IsNone || !e.NewItem.Data.IsFuel) return;

            if (Crafter.Routines.Any() && Crafter.Routines.First().Status == Paused)
            {
                Crafter.Routines.First().Start();
            }
        }
        
        /// <summary>
        /// Tries to consume fuel if available
        /// </summary>
        /// <returns></returns>
        protected virtual bool TryConsumeFuel(float amount)
        {
            if (fuelRemainingSeconds > 0)
            {
                fuelRemainingSeconds -= amount;
                OnFuelUpdateEvent(fuelRemainingSeconds);
                return true;
            }

            /// check for available fuel on fuel container, then consume
            if (!HasFuelItem ||
                !FuelContainer.TryGetNearestFuel(1, out Item fuel))
            {
                OnFuelDepletedEvent();
                return false;
            }
            
            lastFuelMaxDurationSeconds = fuel.Data.FuelDurationSeconds;
            fuelRemainingSeconds = fuel.Data.FuelDurationSeconds;
            OnConsumeFuelEvent(fuel.Data.FuelDurationSeconds);
            return true;
        }

        protected virtual void OnConsumeFuelEvent(float value)
        {
            OnConsumeFuel?.Invoke(value);
        }

        protected virtual void OnFuelUpdateEvent(float value)
        {
            OnFuelUpdate?.Invoke(value);
        }

        protected virtual void OnFuelDepletedEvent()
        {
            OnFuelDepleted?.Invoke();
            
            Crafter.PauseAll();
        }
    }
}