using UnityEngine;
using URMG.Items;

namespace URMG.Crafting
{
    public class RecipeData : ScriptableObject
    {
        Item Output;

        public RecipeData()
        {

        }
    }

    public struct MaterialsData
    {
        Item[] Items;

        public MaterialsData(Item[] list)
        {
            Items = list;
        }
    }
}
