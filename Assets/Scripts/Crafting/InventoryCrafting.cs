using System;
using System.Collections.Generic;
using System.Linq;
using UZSG.Data;
using UZSG.Entities;
using UZSG.Inventory;
using UZSG.Items;

namespace UZSG.Crafting
{
    /// <summary>
    /// Crafting logic that is based on input containers.
    /// </summary>
    public class InventoryCrafting : Crafter
    {
        public void CraftQueue(Container input, Container output, RecipeData recipe, CraftingRoutine craftingRoutine)
        {
            // craftingRoutine.OnCraftFinished += OnCraftFinish;
            // craftingRoutine.OnCraftSecond += OnCraftSeconds;
            // craftingRoutine.RoutineList.Add(craftingRoutine);
            StartCoroutine(craftingRoutine.StartCraftCoroutine());
        }

        public void CraftItem(RecipeData recipe, Container input, Container output, List<CraftingRoutine> routineList)
        {
            if (!CheckMaterialAvailability(recipe, input)) return;

            var materials = TakeItems(recipe, input);
            var craftRoutineConf = new CraftingRoutineOptions()
            {
                Recipe = recipe,
                Output = output,
                RoutineList = routineList,
                MaterialSets = materials
            };

            var newCraftingInstance = new CraftingRoutine(craftRoutineConf, false);
            CraftQueue(input, output, recipe, newCraftingInstance);
        }

        public void CancelCraftItem(List<CraftingRoutine> routineList, Container input, CraftingRoutine routine)
        {
            /// Check first if inventory is able to return the items before cancelling container
            if (CanReturnItems(input, routine.Recipe.Materials.ToList()))
            {
                StopCoroutine(routine.StartCraftCoroutine());
                routineList.Remove(routine);
            }
            /// Return items here vv
        }
    }
}