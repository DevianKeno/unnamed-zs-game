using UZSG.Data;
using UZSG.Inventory;

namespace UZSG.Crafting
{
    public struct CraftItemOptions
    {
        /// <summary>
        /// The recipe to be crafted.
        /// </summary>
        public RecipeData Recipe { get; set; }
        /// <summary>
        /// Amount of times to craft the Recipe.
        /// </summary>
        public int Count { get; set; }
        public ItemSlot QueueSlot { get; set; }
        public ItemSlot OutputSlot { get; set; }
    }
}