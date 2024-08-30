using System;
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
        public int Rounds;
        public readonly bool HasAmmo => Rounds != 0;
        public bool HasMagazine;
        public bool HasBulletInChamber;
    }

    public class GunItemEntity : ItemEntity
    {
        public GunItemEntityInfo Info;
    }
}