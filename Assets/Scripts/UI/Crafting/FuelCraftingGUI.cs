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

public class FuelCraftingGUI : WorkstationGUI
{

    public ItemSlotUI fuelSlot;
    public FuelBar fuelBar;

    FuelBasedCrafting FuelBasedCrafterInstance;
    protected void CreateFuelSlotUI()
    {

    }

    //links specifically to FuelBasedCrafting type of crafter
    public override void LinkWorkstation(Workstation workstation)
    {
        Workstation = workstation;

        FuelBasedCrafterInstance = (FuelBasedCrafting)workstation.Crafter;
        FuelBasedCrafterInstance.FuelContainer = new(1);
        
        FuelBasedCrafterInstance.OnFuelUpdate += OnFuelUpdate;
        FuelBasedCrafterInstance.OnFuelReload += OnFuelReload;
        fuelBar.fuelBasedCraftingInstance = FuelBasedCrafterInstance;
        fuelBar.Value = 0;
        
        CreateOutputSlotUIs(workstation.WorkstationData.OutputSize);
        CreateQueueSlotUIs(workstation.WorkstationData.QueueSize);

        fuelSlot.Link(FuelBasedCrafterInstance.FuelContainer.Slots[0]);
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

            if (player.InventoryWindow.IsHoldingItem)
            {
                if(!player.InventoryWindow.HeldItem.Data.IsFuel)
                {
                    print("You cannot put a non-fuel Item in the fuel slot");
                    return;
                }

                var heldItem = player.InventoryWindow.HeldItem;

                if (slot.IsEmpty || slot.Item.CompareTo(heldItem))
                {
                    slot.TryStack(player.InventoryWindow.TakeHeldItem(), out var excess);
                    if (!excess.IsNone)
                    {
                        player.InventoryWindow.HoldItem(excess);
                    }
                    else
                    {
                        //if there is a routine still ongoing, ignite fuel
                        //how tho...
                        if (FuelBasedCrafterInstance.Routines.Count > 0 && !FuelBasedCrafterInstance.IsFuelRemainingAvailable())
                        {
                            if (FuelBasedCrafterInstance.TryConsumeFuel())
                            {
                                FuelBasedCrafterInstance.StartBurn();
                            }
                            FuelBasedCrafterInstance.ContinueRemainingCraft();
                        }
                    }
                }
                else /// item diff, swap
                {
                    var tookItem = slot.TakeAll();
                    var prevHeld = player.InventoryWindow.SwapHeldWith(tookItem);
                    slot.Put(prevHeld);
                }
            }
            else
            {
                if (slot.IsEmpty) return;

                player.InventoryWindow.HoldItem(slot.TakeAll());
                // _lastSelectedSlot = slot;
            }
        }
        else if (ctx.Button == Right)
        {
            if (player.InventoryWindow.IsHoldingItem)
            {
                if(!player.InventoryWindow.HeldItem.Data.IsFuel)
                {
                    print("You cannot put a non-fuel Item in the fuel slot");
                    return;
                }
                
                var heldItem = player.InventoryWindow.HeldItem;

                if (slot.IsEmpty || slot.Item.CompareTo(heldItem))
                {
                    slot.TryStack(player.InventoryWindow.TakeHeldItemSingle(), out var excess);
                    if (!excess.IsNone)
                    {
                        player.InventoryWindow.HoldItem(excess);
                    }
                    else
                    {
                        //if there is a routine still ongoing, ignite fuel
                        //how tho...
                        if (FuelBasedCrafterInstance.Routines.Count > 0 && !FuelBasedCrafterInstance.IsFuelRemainingAvailable())
                        {
                            if (FuelBasedCrafterInstance.TryConsumeFuel())
                            {
                                FuelBasedCrafterInstance.StartBurn();
                            }
                            FuelBasedCrafterInstance.ContinueRemainingCraft();
                        }
                    }
                }
                else /// item diff, swap
                {
                    var tookItem = slot.TakeAll();
                    var prevHeld = player.InventoryWindow.SwapHeldWith(tookItem);
                    slot.Put(prevHeld);
                }
            }
            else
            {
                if (slot.IsEmpty) return;

                player.InventoryWindow.HoldItem(slot.TakeAll());
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
