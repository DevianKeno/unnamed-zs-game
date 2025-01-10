using System;
using Unity.VisualScripting;
using UnityEngine;
using UZSG.Crafting;

namespace UZSG.UI
{
    public class FuelBar : ProgressBarUI
    {
        public FuelBasedCrafting fuelBasedCraftingInstance;
        private float _maxFuel;

        public void UpdateMaxFuel(float fuelCount)
        {
            _maxFuel = fuelCount * 1000;
        }
        public void UpdateFuelRemaining()
        {
            float _percent = (fuelBasedCraftingInstance.FuelRemaining / _maxFuel) * 100;
            Value = _percent;
            RefreshBar();
        }
    }
}