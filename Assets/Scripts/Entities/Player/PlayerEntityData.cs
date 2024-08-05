using System;
using System.Collections.Generic;

using UnityEngine;

using UZSG.Attributes;
using UZSG.Crafting;
using UZSG.Data;

namespace UZSG.Entities
{
    /// <summary>
    /// EntityData class made specifically for Player entities.
    /// </summary>
    [Serializable]
    [CreateAssetMenu(fileName = "New Player Entity Data", menuName = "UZSG/Entity/Player Entity Data")]
    public class PlayerEntityData : EntityData
    {        
        [Header("Attributes")]
        public List<AttributeData> Vitals;
        public List<AttributeData> Generic;
        public List<RecipeData> KnownRecipes;
    }
}