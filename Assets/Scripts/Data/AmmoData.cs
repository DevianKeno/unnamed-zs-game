using System;

using UnityEngine;

namespace UZSG.Data
{
    [Serializable]
    public struct AmmoBaseAttributes
    {
        public float Damage;
        public float Velocity;
    }

    /// <summary>
    /// Ammo data.
    /// Values are set in Inspector; <b>Do not write</b>.
    /// </summary>
    [Serializable]
    [CreateAssetMenu(fileName = "New Item Data", menuName = "UZSG/Items/Ammo Data")]
    public class AmmoData : ItemData
    {
        /// <summary>
        /// Ammo caliber.
        /// </summary>
        [Header("Ammo Data")]
        public string Caliber;
    }
}