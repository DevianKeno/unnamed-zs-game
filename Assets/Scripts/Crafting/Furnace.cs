using System;
using UnityEditor.Build;
using UnityEngine;
using UZSG.Data;
using UZSG.Items;

namespace UZSG.Crafting
{
    public class Furnace : FuelBasedCrafting
    {
        public Container ConsumableContainer = new(3);
        public int FurnaceCapacity = 3;

        public void Init(){
            // InputContainers.Add(new Container(5));
        }

        protected bool isFurnaceFull(){
            if(craftingRoutineList.Count >= FurnaceCapacity){
                return true;
            }
            return false;
        }

        public void StartCookingConsumable(Item item){
            if (!isFurnaceFull()){
                print("Furnace is still busy");
            }
        }
    }
}

    