using System.Collections.Generic;
using UnityEngine;
using UZSG.Attributes;

namespace UZSG.Player
{
    /// <summary>
    /// Represents the Player's vital attributes (Health, Stamina, etc).
    /// </summary>
    [System.Serializable]
    public class PlayerVitalAttributes
    {
        Dictionary<string, Attribute> _attributes;

        [SerializeField] VitalAttribute _health;
        public VitalAttribute Health => _health;

        public Attribute this[string name]
        {
            get => _attributes[name];
        }

        internal void Initialize()
        {
        }

        public PlayerVitalAttributes()
        {
            _attributes = new()
            {
                // {"Health", VitalAttribute.Health},
                // {"Stamina", VitalAttribute.Stamina},
                // // {"Mana", VitalAttributes.Mana},
                // {"Hunger", VitalAttribute.Hunger},
                // {"Hydration", VitalAttribute.Hydration}
            };
        }

    }
}
