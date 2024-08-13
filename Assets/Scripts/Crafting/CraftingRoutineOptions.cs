using System.Collections.Generic;

using UZSG.Items;
using UZSG.Data;

namespace UZSG.Crafting
{
    public struct CraftingRoutineOptions
    {
        public RecipeData Recipe { get; set ;}
        public List<Item> MaterialSets { get; set ;}
        public Container Output { get; set ;}
        public List<CraftingRoutine> RoutineList { get; set ;}

        // public CraftingRoutineOptions(RecipeData recipe, List<Item> materialSets, Container output, List<CraftingRoutine> routineList)
        // {
        //     this.recipe = recipe;
        //     this.materialSets = materialSets;
        //     this.output = output;
        //     this.routineList = routineList;
        // }
    }
}