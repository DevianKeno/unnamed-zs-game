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
    public class InventoryCrafting : Crafter
    {

        public void CraftQueue(Container input, Container output, RecipeData recipe, CraftingRoutine craftingRoutine)
        {
            craftingRoutine.OnCraftFinish += OnCraftFinish;
            craftingRoutine.OnCraftSecond += OnCraftSeconds;
            craftingRoutine.routineList.Add(craftingRoutine);
            StartCoroutine(craftingRoutine.CraftCoroutine());
        }

        public void CraftItem(RecipeData recipe, Container input, Container output, List<CraftingRoutine> routineList) {
            if(!CheckMaterialAvailability(recipe, input)) return;
            var _materials = TakeItems(recipe, input);
            var _craftRoutineConf = new CraftingRoutineOptions() {
                recipe = recipe,
                output = output,
                routineList = routineList,
                materialSets = _materials
            };
            var _newCraftingInstance = new CraftingRoutine(_craftRoutineConf);
            CraftQueue(input, output, recipe, _newCraftingInstance);
        }

        public void CancelCraftItem(List<CraftingRoutine> routineList, Container input, CraftingRoutine routine){
            //check first if inventory is able to return the items before cancelling container
            ReturnItems(input, routine.materialSets);
            StopCoroutine(routine.CraftCoroutine());
            routineList.Remove(routine);
        }
    }
}