using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UZSG
{
    public class Stats
    {
        Stat _armor;
        public Stat Armor { get => _armor; }
        Stat _resist;
        public Stat Resist { get => _resist; }
        Stat _critChance;
        public Stat CritChance { get => _critChance; }
        Stat _critDamage;
        public Stat CritDamage { get => _critDamage; }
        Stat _damageReduc;
        public Stat DamageReduc { get => _damageReduc; }
        Stat _damageBoost;
        public Stat DamageBoost { get => _damageBoost; }
    }
}
