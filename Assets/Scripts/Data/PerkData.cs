using System;

using UnityEngine;

namespace UZSG.Data.Perks
{
    public enum PerkTier {
        Basic, Epic, Unique
    }

    /// <summary>
    /// Particle data.
    /// Values are set in Inspector; <b>Do not write</b>.
    /// </summary>
    [Serializable]
    [CreateAssetMenu(fileName = "New Perk Data", menuName = "UZSG/Perk Data")]
    public class PerkData : BaseData
    {
        [Header("Perk Data")]
        public string Name;
        [TextArea] public string Description;
        public Sprite Icon;
        public PerkTier Tier;
        public int PointCost;
        public int MaxLevel;
    }
}