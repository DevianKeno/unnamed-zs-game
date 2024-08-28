using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UZSG.Crafting;
using UZSG.Objects;
using UZSG.UI;
using UZSG.UI.Objects;


using static UnityEngine.EventSystems.PointerEventData.InputButton;
using static UZSG.UI.ItemSlotUI.ClickType;

public class FuelCraftingGUI : CraftingGUI
{

    public ItemSlotUI fuelSlot;
    public FuelBar fuelBar;

    protected void CreateFuelSlotUI()
    {

    }

    //links specifically to FuelBasedCrafting type of crafter
    public override void LinkWorkstation(Workstation workstation)
    {
        this.workstation = workstation;

        var fbc = (FuelBasedCrafting)workstation.Crafter;
        fbc.FuelContainer = new(1);

        fbc.OnFuelUpdate += OnFuelUpdate;
        fbc.OnFuelReload += OnFuelReload;
        fuelBar.fuelBasedCraftingInstance = fbc;
        fuelBar.Value = 0;
        

        CreateOutputSlotUIs(workstation.WorkstationData.OutputSize);
        CreateQueueSlotUIs(workstation.WorkstationData.QueueSize);

        fuelSlot.Link(fbc.FuelContainer.Slots[0]);
        fuelSlot.OnMouseDown += OnFuelSlotClick;
    }

    //A series of decision tree for fuel slot logic 
    private void OnFuelSlotClick(object sender, ItemSlotUI.ClickedContext ctx)
    {

        var slot = ((ItemSlotUI) sender).Slot;

        if (ctx.Button == Left)
        {
            if (ctx.ClickType == ShiftClick)
            {
                player.Inventory.Bag.TryPutNearest(slot.TakeAll());
                return;
            }

            if (player.InventoryGUI.IsHoldingItem)
            {
                var heldItem = player.InventoryGUI.HeldItem;
                var fbc = (FuelBasedCrafting)workstation.Crafter;

                if (slot.IsEmpty || slot.Item.CompareTo(heldItem))
                {
                    slot.TryCombine(player.InventoryGUI.TakeHeldItem(), out var excess);
                    if (!excess.IsNone)
                    {
                        player.InventoryGUI.HoldItem(excess);
                    }
                    else
                    {
                        //if there is a routine still ongoing, ignite fuel
                        //how tho...
                        if (fbc.Routines.Count > 0 && !fbc.IsFuelRemainingAvailable())
                        {
                            if (fbc.TryConsumeFuel())
                            {
                                fbc.StartBurn();
                            }
                            fbc.ContinueRemainingCraft();
                        }
                    }
                }
                else /// item diff, swap
                {
                    var tookItem = slot.TakeAll();
                    var prevHeld = player.InventoryGUI.SwapHeldWith(tookItem);
                    slot.Put(prevHeld);
                }
            }
            else
            {
                if (slot.IsEmpty) return;

                player.InventoryGUI.HoldItem(slot.TakeAll());
                // _lastSelectedSlot = slot;
            }
        }
    }


    //An event function called by the FuelBasedCrafting whenever it sends back its fuel status 
    private void OnFuelUpdate()
    {
        fuelBar.UpdateFuelRemaining();
    }

    //An event function called by the FuelBasedCrafting whenever it consumes a new fuel
    private void OnFuelReload(float fuelDuration)
    {
        fuelBar.UpdateMaxFuel(fuelDuration);
    }
}
