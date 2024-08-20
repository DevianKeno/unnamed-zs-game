using System;
using System.Collections.Generic;
using UnityEngine;
using UZSG.Items;

namespace UZSG.Data
{
    [Serializable]
    [CreateAssetMenu(fileName = "New Recipe Data", menuName = "UZSG/Recipe Data")]
    public class RecipeData : BaseData
    {
        public string Name;
        public Item Output;
        public int Yield => Output.Count;
        public List<Item> Materials;
        public bool RequiresFuel = false;
        public float DurationSeconds;
    }
}
