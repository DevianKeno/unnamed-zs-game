using System;

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
}