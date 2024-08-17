using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace UZSG.Data
{
    [Serializable]
    [CreateAssetMenu(fileName = "New Station Data", menuName = "UZSG/Objects/Station Data")]
    public class WorkstationData : ObjectData
    {
        [Header("Workstation Data")]
        public string WorkstationName;
        public AssetReference GUI;
        public bool IncludePlayerRecipes;
        public int QueueSize;
        public int OutputSize;
        public List<RecipeData> IncludedRecipes;
    }
}