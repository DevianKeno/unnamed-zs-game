using System;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters;
using UnityEditorInternal.Profiling.Memory.Experimental;
using UnityEngine;
using UnityEngine.ResourceManagement.Util;
using UZSG.Data;
using UZSG.Entities;
using UZSG.Inventory;
using UZSG.Items;

namespace UZSG.Crafting 
{
    public struct CraftFinishedInfo 
    {
            public DateTime StartTime;
            public DateTime EndTime;
    }
    public class InventoryCrafting : Crafter
    {
        public Player PlayerEntity;

        public List<CraftingRoutine> craftingRoutineList;

        public void CraftQueue(RecipeData recipe)
        {
            var dictSlots = new Dictionary<Item, List<ItemSlot>>();

            if ( !CheckMaterialAvailability(recipe, dictSlots))
            {
                return;
            }

            TakeItems(recipe, dictSlots);

            CraftingRoutine craftInstance = new CraftingRoutine(recipe);

            craftInstance.OnCraftFinish += OnCraftFinish;
            craftInstance.OnCraftSecond += OnCraftSeconds;

            StartCoroutine(craftInstance.CraftCoroutine());
        }

        private void OnCraftSeconds(object sender, int secondsElapsed)
        {
            var x = (CraftingRoutine) sender;
            print($"Crafting: {x.RecipeData.Id} - {x.RecipeData.DurationSeconds - secondsElapsed} seconds Remaining");
        }

        private void OnCraftFinish(object sender, CraftFinishedInfo unixTime)
        {
            var x = (CraftingRoutine) sender;
            containers[0].TryPutNearest(new Item(x.RecipeData.Output));
        }

    }
}