using UnityEngine;
using URMG.Items;

namespace URMG.Crafting
{
    [CreateAssetMenu(fileName = "Recipe Data", menuName = "URMG/Recipe Data")]
    public class RecipeData : ScriptableObject
    {
        public string Name;
        public Item Output;
        public Item[] Materials;
    }
}
