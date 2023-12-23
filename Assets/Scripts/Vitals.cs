using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UZSG
{
    /// <summary>
    /// Represents the player's vital statistics.
    /// </summary>
    [Serializable]
    public class Vitals
    {
        [SerializeField] VitalAttribute health;
        public VitalAttribute Health { get => health; }

        [SerializeField] VitalAttribute _stamina;
        public VitalAttribute Stamina { get => _stamina; }

        [SerializeField] VitalAttribute _mana;
        public VitalAttribute Mana { get => _mana; }

        [SerializeField] VitalAttribute _hunger;
        public VitalAttribute Hunger { get => _hunger; }

        [SerializeField] VitalAttribute _thirst;
        public VitalAttribute Thirst { get => _thirst; }

        [SerializeField] VitalAttribute _infection;
        public VitalAttribute Infection { get => _infection; }
        
        [SerializeField] VitalAttribute _oxygen;
        public VitalAttribute Oxygen { get => _oxygen; }

        public Vitals()
        {
            health = new(100f, 100f, VitalAttribute.Cycle.Regen);
            _stamina = new(100f, 100f, VitalAttribute.Cycle.Regen);
            _mana = new(100f, 100f, VitalAttribute.Cycle.Regen);
            _hunger = new(100f, 100f, VitalAttribute.Cycle.Degen);
            _thirst = new(100f, 100f, VitalAttribute.Cycle.Degen);
            _infection = new(100f, 0f, VitalAttribute.Cycle.Static);
            _oxygen = new(100f, 100f, VitalAttribute.Cycle.Static);
        }
    }
}