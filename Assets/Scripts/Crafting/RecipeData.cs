using UnityEngine;
using UZSG.Items;

namespace UZSG.Crafting
{
    [CreateAssetMenu(fileName = "New Recipe Data", menuName = "UZSG/Recipe Data")]
    public class RecipeData : ScriptableObject
    {
        public string Id;
        public string Name;
        public Item Output;
        public Item[] Materials;
    }
}
