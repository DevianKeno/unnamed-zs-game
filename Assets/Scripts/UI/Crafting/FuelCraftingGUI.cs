using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UZSG.Crafting;
using UZSG.Objects;
using UZSG.UI;
using UZSG.UI.Objects;

public class FuelCraftingGUI : CraftingGUI
{

    public ItemSlotUI fuelSlot;

    public override void LinkWorkstation(Workstation workstation)
    {
        this.workstation = workstation;

        var fbc = (FuelBasedCrafting)workstation.Crafter;

        CreateOutputSlotUIs(workstation.WorkstationData.OutputSize);
        CreateQueueSlotUIs(workstation.WorkstationData.QueueSize);

        fuelSlot.Link(fbc.FuelContainer.Slots[0]);
        fuelSlot.OnMouseDown += OnFuelSlotClick;
    }

    private void OnFuelSlotClick(object sender, ItemSlotUI.ClickedContext ctx)
    {
        var slot = ((ItemSlotUI) sender).Slot;

            if (ctx.Pointer.button == PointerEventData.InputButton.Left)
            {
                if (slot.IsEmpty) return;
                if (player.InventoryGUI.IsHoldingItem)
                {
                    //fdfsdf
                }

                if (ctx.ClickType == ItemSlotUI.ClickType.FastDeposit)
                {
                    player.Inventory.Bag.TryPutNearest(slot.TakeAll());
                }
                else
                {
                    player.InventoryGUI.HoldItem(slot.TakeAll());
                }
            }
    }
}
