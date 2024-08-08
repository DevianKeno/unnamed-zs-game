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
        public Container InputContainer;
        public int FurnaceCapacity = 3;

        protected bool isFurnaceFull(){
            if(craftingRoutineList.Count >= FurnaceCapacity){
                return true;
            }
            return false;
        }

        public void StartCooking(RecipeData recipe){
            if (!isFurnaceFull()){
                print("Furnace is still busy");
                return;
            }

            if(!CheckMaterialAvailability(recipe, InputContainer)){
                print("Not enough materials");
                return;
            };
            var _ingredients = TakeItems(recipe, InputContainer);
            var _beingCooked = new CraftingRoutine(recipe, _ingredients, Dishes);
        }
    }
}

    