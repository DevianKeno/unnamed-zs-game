using System;
using UZSG.Data;

namespace UZSG.Items.Weapons
{    
    [Serializable]
    public struct WeaponRangedAttributes
    {
        public FiringModes FiringModes;
        public int RoundsPerMinute;
        public int ClipSize;
        public float ReloadSpeed;
        /// <summary>
        /// TODO: change into a more robust system
        /// The Id of this gun's bullets.
        /// </summary>
        public AmmoData Ammo;
        public float Spread;
        public int BurstFireCount;
        public float BurstFireInterval;
        public BulletDamageAttributes BulletDamage;
        public BulletAttributes BulletAttributes;
        public RecoilAttributes RecoilAttributes;
    }
}
