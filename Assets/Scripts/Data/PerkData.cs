using System;

using UnityEngine;

namespace UZSG.Data.Perks
{
    [Serializable]
    [CreateAssetMenu(fileName = "New Perk Data", menuName = "UZSG/Perk Data")]
    public class PerkData : BaseData
    {
        [Header("Perk Data")]
        public string Name;
        [TextArea] public string Description;
        public Sprite Icon;
        public int PointCost;
        public int MaxLevel;
    }
}