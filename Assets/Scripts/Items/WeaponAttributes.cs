using System;
using UnityEngine.Rendering;

namespace UZSG.Items.Weapons
{
    [Serializable]
    public struct WeaponMeleeAttributes
    {
        public float BaseDamage;
        public float BaseCritStrike;
        public float BaseCritDamage;
    }

    public enum FireType {
        SemiAuto, Automatic, Burst
    }
    
    [Serializable]
    public struct WeaponRangedAttributes
    {
        public float Damage; /// Can be further broken down into body parts damage
        public FireType FireType;
        public int RoundsPerMinute;
        public int ClipSize;
        public float ReloadSpeed;
        public float BulletVelocity;
        public float VerticalRecoilFactor;
        public float HorizontalRecoilFactor;
        public float RecoilRecoveryFactor;
    }
}
