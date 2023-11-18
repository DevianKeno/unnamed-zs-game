using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace ZS
{
    public class Attributes
    {
        Dictionary<string, Attribute> attributes;
        
        GenericAttribute _movementSpeed;
        public GenericAttribute MovementSpeed { get => _movementSpeed; }
        GenericAttribute _experience;
        public GenericAttribute Experience { get => _experience; }
        Health _health;
        public Health Health { get => _health; }
        VitalAttribute _stamina;
        public VitalAttribute Stamina { get => _stamina; }
        VitalAttribute _mana;
        public VitalAttribute Mana { get => _mana; }

        public float this[string name]
        {
            get => attributes[name].Value;
        }

        public Attributes()
        {
            
        }
    }
}
