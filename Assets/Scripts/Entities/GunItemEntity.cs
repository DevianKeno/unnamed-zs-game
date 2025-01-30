using System;
using UnityEngine.Serialization;
using UZSG.Interactions;
using UZSG.Items.Weapons;

namespace UZSG.Entities
{
    /// <summary>
    /// Holds information on Guns dropped on the ground as Items.
    /// </summary>
    [Serializable]
    public struct GunItemEntityInfo
    {
        public int Rounds { get; set; }
        public readonly bool HasAmmo => Rounds > 0;
        public bool HasMagazine { get; set; }
        public bool IsChambered { get; set; }
    }

    public class GunItemEntity : ItemEntity
    {
        [FormerlySerializedAs("Info")] public GunItemEntityInfo GunInfo;
    }
}