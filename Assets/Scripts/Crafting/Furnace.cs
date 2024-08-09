using System;
using System.Collections.Generic;
using UnityEditor.Build;
using UnityEngine;
using UZSG.Data;
using UZSG.Inventory;
using UZSG.Items;

namespace UZSG.Crafting
{
    public class Furnace : FuelBasedCrafting
    {
        public Container Dishes = new(5);
        public int FurnaceCapacity = 3;

        List<CraftingRoutine> furnaceRoutineList = new();
        protected bool isFurnaceFull()
        {
            if(furnaceRoutineList.Count >= FurnaceCapacity){
                return true;
            }
            return false;
        }

        public void PrepareCooking(RecipeData recipe){
            if (!isFurnaceFull()){
                print("Furnace is still busy");
                return;
            }

            if(!CheckMaterialAvailability(recipe, InputContainer)){
                print("Not enough materials");
                return;
            };
            var _ingredients = TakeItems(recipe, InputContainer);

            var _fuelRoutineConf = new CraftingRoutineOptions() {
                recipe = recipe,
                materialSets = _ingredients,
                output = Dishes,
                routineList = furnaceRoutineList,
            };
            
            var _cookInstance = new CraftingRoutine(_fuelRoutineConf, true);
            furnaceRoutineList.Add(_cookInstance);
        }

        public void StartCooking(){
            ConsumeFuel();
            if (isFuelRemainingAvailable()){
                foreach (var routine in furnaceRoutineList) {
                    StartCoroutine(routine.CraftCoroutine());
                }
            }
        }

    }
}

    