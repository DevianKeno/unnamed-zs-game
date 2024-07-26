using System;

namespace UZSG.Items.Weapons
{    
    [Serializable]
    public struct WeaponRangedAttributes
    {
        public FiringModes FiringModes;
        public int RoundsPerMinute;
        public int ClipSize;
        public float ReloadSpeed;
        public float Spread;
        public int BurstFireCount;
        public float BurstFireInterval;
        public BulletDamageAttributes BulletDamage;
        public BulletAttributes BulletAttributes;
        public RecoilAttributes RecoilAttributes;
    }
}
