using System.Collections.Generic;

using UZSG.Items;
using UZSG.Objects;

namespace UZSG.Crafting
{
    public static class CraftingUtils
    {
        /// <summary>
        /// Takes crafting options and calculates the total amount of items needed to craft that quantity.
        /// </summary>
        /// <returns>List of items</returns>
        public static List<Item> CalculateTotalMaterials(CraftItemOptions options)
        {
            var list = new List<Item>();
            var mats = options.Recipe.Materials;

            for (int i = 0; i < mats.Count; i++)
            {
                var mat = new Item(mats[i]) * options.Count;
                list.Add(mat);
            }

            return list;
        }

        public static void PlayCraftSound(CraftingStation source)
        {
            if (source.GUI != null && source.GUI.IsVisible)
            {
                Game.Audio.PlayInUI("craft");
            }
        }

        public static void PlayNoMaterialsSound(CraftingStation source)
        {
            if (source.GUI != null && source.GUI.IsVisible)
            {
                Game.Audio.PlayInUI("insufficient_materials");
            }
        }
    }
}