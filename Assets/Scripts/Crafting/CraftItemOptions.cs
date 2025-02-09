using System;

using UZSG.Data;

namespace UZSG.Crafting
{
    [Serializable]
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
    }
}