using System;
using System.Collections.Generic;

namespace UZSG.Items.Weapons
{
    [Serializable]
    public struct BulletDamageAttributes
    {
        public float BaseDamage;
        public bool IsPartDamage;
        public bool UseMultiplier;
        public float HeadDamage;
        public float HeadDamageMultiplier;
        public float BodyDamage;
        public float BodyDamageMultiplier;
        public float ArmsDamage;
        public float ArmsDamageMultiplier;
        public float LegsDamage;
        public float LegsDamageMultiplier;
    }
}