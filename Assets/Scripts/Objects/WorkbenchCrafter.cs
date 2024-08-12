using System.Collections.Generic;
using Mono.Cecil.Cil;
using UZSG.Data;
using UZSG.Entities;

namespace UZSG.Crafting
{
    public class WorkbenchCrafter : InventoryCrafting
    {
        public Container OutputContainer = new(5);
        public List<CraftingRoutine> WorkbenchCraftingList = new();

        public void WorkbenchCraftItem(Player player, RecipeData recipe){
            var PlayerInventory = player.Inventory.Bag;
            CraftItem(recipe, PlayerInventory, OutputContainer, WorkbenchCraftingList);
        }
    }
}