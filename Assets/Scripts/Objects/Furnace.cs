using System;
using System.Collections.Generic;

using UnityEngine;

using UZSG.Data;
using UZSG.Objects;

namespace UZSG.Crafting
{
    public class Furnace : FueledWorkstation
    {
        // public Container Dishes = new(5);
        // public int FurnaceCapacity = 3;

        // List<CraftingRoutine> furnaceRoutineList = new();

        // protected bool IsFurnaceFull
        // {
        //     get
        //     {
        //         return furnaceRoutineList.Count >= FurnaceCapacity;
        //     }
        // }

        // public void PrepareCooking(RecipeData recipe)
        // {
        //     if (!IsFurnaceFull)
        //     {
        //         print("Furnace is still busy");
        //         return;
        //     }

        //     if(!CheckMaterialAvailability(recipe, InputContainer))
        //     {
        //         print("Not enough materials");
        //         return;
        //     };

        //     var ingredients = TakeItems(recipe, InputContainer);
        //     var fuelRoutineConf = new CraftingRoutineOptions()
        //     {
        //         Recipe = recipe,
        //         MaterialSets = ingredients,
        //         Output = Dishes,
        //         RoutineList = furnaceRoutineList,
        //     };
            
        //     var cookInstance = new CraftingRoutine(fuelRoutineConf, true);
        //     furnaceRoutineList.Add(cookInstance);
        // }

        // public void StartCooking()
        // {
        //     ConsumeFuel();
        //     if (IsFueled)
        //     {
        //         foreach (var routine in furnaceRoutineList)
        //         {
        //             routine.Start();
        //         }
        //     }
        // }
    }
}

    