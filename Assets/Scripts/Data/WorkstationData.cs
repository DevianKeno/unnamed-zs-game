using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace UZSG.Data
{
    [Serializable]
    [CreateAssetMenu(fileName = "New Station Data", menuName = "UZSG/Station Data")]
    public class WorkstationData : ObjectData
    {
        [Header("Workstation")]
        public string WorkstationName;
        public AssetReference GUI;
        public List<RecipeData> IncludedRecipes;
    }
}