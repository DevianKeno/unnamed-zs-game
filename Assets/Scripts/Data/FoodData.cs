using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UZSG.Attributes;

namespace UZSG.Data
{
    [Serializable]
    public struct RecoveryData 
    {
        public AttributeData Attribute;
        public int Restore;
        public bool IsOverTime;
    }

    [Serializable]
    public struct FoodStatus
    {
        public bool CanBeCooked;
        public bool IsBurnt;
        public bool CanSpoil;
    }
    
    /// <summary>
    /// Food data.
    /// Values are set in Inspector; <b>Do not write</b>.
    /// </summary>
    [CreateAssetMenu(fileName = "New Food Data", menuName = "UZSG/Food Data")]
    [Serializable]
    public class FoodData : ItemData
    {
        public List<RecoveryData> recoveredAttributes;
        public FoodStatus foodStatus;
    }
}