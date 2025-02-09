using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.AddressableAssets;

namespace UZSG.Data
{
    /// <summary>
    /// Data for workstation.
    /// Values are set in Inspector; <b>Do not write</b>.
    /// </summary>
    [Serializable]
    [CreateAssetMenu(fileName = "New Workstation Data", menuName = "UZSG/Objects/Workstation Data")]
    public class WorkstationData : ObjectData
    {
        [Header("Workstation Data")]
        public string WorkstationName;
        public string WorkstationNameTranslatable => Game.Locale.Translatable($"workstation.{Id}.name");
        public AssetReference GUIAsset;
        public bool IncludePlayerRecipes;
        public int QueueSize;
        public int OutputSize;
        public bool RequiresFuel;
        public int FuelSlotsSize;
        /// <summary>
        /// Whether to consume fuel even when idling.
        /// </summary>
        public bool UsesFuelWhenIdle;
        public List<RecipeData> IncludedRecipes;
    }
}