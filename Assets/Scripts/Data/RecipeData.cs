using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Serialization;

using UZSG.Items;

namespace UZSG.Data
{
    /// <summary>
    /// Recipe data.
    /// Values are set in Inspector; <b>Do not write</b>.
    /// </summary>
    [Serializable]
    [CreateAssetMenu(fileName = "New Recipe Data", menuName = "UZSG/Recipe Data")]
    public class RecipeData : BaseData
    {
        [FormerlySerializedAs("Name")] public string DisplayName;
        public string DisplayNameTranslatable => Game.Locale.Translatable($"recipe.{Id}.name");

        public string Description;
        public string DescriptionTranslatable => Game.Locale.Translatable($"recipe.{Id}.description");

        public Item Output;
        public int Yield => Output.Count;
        public List<Item> Materials;
        public bool RequiresFuel = false;
        [FormerlySerializedAs("DurationSeconds")]
        public float CraftingTimeSeconds;
    }
}
