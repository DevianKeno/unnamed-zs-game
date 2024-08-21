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

    protected void CreateFuelSlotUI(){

    }

    public override void LinkWorkstation(Workstation workstation)
    {
        this.workstation = workstation;

        var fbc = (FuelBasedCrafting)workstation.Crafter;
        fbc.FuelContainer = new(1);

        CreateOutputSlotUIs(workstation.WorkstationData.OutputSize);
        CreateQueueSlotUIs(workstation.WorkstationData.QueueSize);

        fuelSlot.Link(fbc.FuelContainer.Slots[0]);
        fuelSlot.OnMouseDown += OnFuelSlotClick;
    }

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

                if (slot.IsEmpty || slot.Item.CompareTo(heldItem))
                {
                    slot.TryCombine(player.InventoryGUI.TakeHeldItem(), out var excess);
                    if (!excess.IsNone)
                    {
                        player.InventoryGUI.HoldItem(excess);
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
}
