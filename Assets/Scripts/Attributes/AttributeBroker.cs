using System;
using System.Linq;
using System.Collections.Generic;

using UnityEngine;

using UZSG.Systems;
using UZSG.Saves;
using UZSG.StatusEffects;

namespace UZSG.Attributes
{
    public class AttributeBroker
    {
        public Attribute Attribute { get; set; }
        public StatusEffectCollection StatusEffects;
        public bool HasStatusEffects { get; private set; }

        public float GetValue()
        {
            return Attribute.Value;
        }
    }
}