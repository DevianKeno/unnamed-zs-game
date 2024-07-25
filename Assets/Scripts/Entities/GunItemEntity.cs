using System;
using UZSG.Items.Weapons;

namespace UZSG.Entities
{
    [Serializable]
    public struct GunItemEntityInfo
    {
        public int Rounds;
        public readonly bool HasAmmo => Rounds != 0;
        public bool HasMagazine;
    }

    public class GunItemEntity : ItemEntity
    {
        public GunWeaponController GunWeapon;
        public GunItemEntityInfo Info;

        protected override void Awake()
        {
            base.Awake();
            GunWeapon = GetComponent<GunWeaponController>();
        }
    }
}