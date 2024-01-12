using System;
using UnityEngine.Rendering;

namespace UZSG.Items
{
    [Serializable]
    public struct WeaponMeleeAttributes
    {
        public float BaseDamage;
        public float BaseCritStrike;
        public float BaseCritDamage;
    }
    
    [Serializable]
    public struct WeaponRangedAttributes
    {
        public float BaseDamage;
        public int Capacity;
        public float ReloadSpeed;
        public float ReloadSpeedMultiplier;
    }
}
