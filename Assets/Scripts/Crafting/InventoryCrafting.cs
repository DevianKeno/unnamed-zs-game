using System;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters;
using UnityEditorInternal.Profiling.Memory.Experimental;
using UnityEngine;
using UnityEngine.ResourceManagement.Util;
using UnityEngine.UI;
using UZSG.Data;
using UZSG.Entities;
using UZSG.Inventory;
using UZSG.Items;

namespace UZSG.Crafting
{
    //test
    public struct CraftFinishedInfo 
    {
            public DateTime StartTime;
            public DateTime EndTime;
    }
 
    public class InventoryCrafting : Crafter
    {
        public List<CraftingRoutine> craftingRoutineList = new();
        public event Action<CraftingRoutine> OnStartCraft;

        
        public void CraftQueue(RecipeData recipe)
        {
            var dictSlots = new Dictionary<Item, List<ItemSlot>>();

            if ( !CheckMaterialAvailability(recipe, dictSlots))
            {
                return;
            }

            TakeItems(recipe, dictSlots);

            CraftingRoutine craftInstance = new(recipe);

            craftInstance.OnCraftFinish += OnCraftFinish;
            craftInstance.OnCraftSecond += OnCraftSeconds;

            craftingRoutineList.Add(craftInstance);

            StartCoroutine(craftInstance.CraftCoroutine());


        }

        public void StopCraftQueue(CraftingRoutine craftingRoutine)
        {
            
        }

        private void OnCraftSeconds(object sender, int secondsElapsed)
        {
            var _craftingInstanceSender = (CraftingRoutine) sender;
            print($"Crafting: {_craftingInstanceSender.RecipeData.Id} - {_craftingInstanceSender.GetTimeRemaining()} seconds Remaining");
        }

        private void OnCraftFinish(object sender, CraftFinishedInfo unixTime)
        {
            var _craftingInstanceSender = (CraftingRoutine) sender;
            containers[0].TryPutNearest(new Item(_craftingInstanceSender.RecipeData.Output));
            craftingRoutineList.Remove(_craftingInstanceSender);
        }

    }
}