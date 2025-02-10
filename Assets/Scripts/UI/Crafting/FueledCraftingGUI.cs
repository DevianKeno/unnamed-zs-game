using UnityEngine;

using UZSG.Objects;

namespace UZSG.UI
{
    public class FueledCraftingGUI : CraftingGUI
    {
        public FueledWorkstation FueledWorkstation => (FueledWorkstation) base.CraftingStation;
        
        [Header("FueledCraftingGUI Elements")]
        [SerializeField] Transform fuelSlotsContainer;
        [SerializeField] FuelBar fuelBar;

        public override void ReadWorkstation(CraftingStation workstation)
        {
            base.ReadWorkstation(workstation);
            CreateFuelSlotUIs(workstation.WorkstationData.FuelSlotsSize);
            
            FueledWorkstation.OnFuelUpdate += OnFuelUpdate;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            FueledWorkstation.OnFuelUpdate -= OnFuelUpdate;
        }

        protected virtual void OnFuelUpdate(float value)
        {
            fuelBar.Value = value / FueledWorkstation.LastFuelMaxDuration;
        }

        protected virtual void CreateFuelSlotUIs(int size)
        {
            for (int i = 0; i < size; i++)
            {
                var slotUI = Game.UI.Create<ItemSlotUI>("Item Slot", parent: fuelSlotsContainer);
                slotUI.name = $"Fuel Slot ({i})";
                slotUI.Link(this.FueledWorkstation.FuelContainer[i]); /// link output container to UI
                slotUI.OnMouseDown += OnFuelSlotClick;
                slotUI.Show();
            }
        }

        protected virtual void OnFuelSlotClick(object sender, ItemSlotUI.ClickedContext click)
        {
            Player.InventoryWindow.OnItemSlotClicked(sender, click);
            return;
        }
    }
}