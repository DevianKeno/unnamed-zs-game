using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ZS
{
    /// <summary>
    /// Vital attributes are stats that regenerate over time.
    /// </summary>
    public abstract class VitalAttribute : Attribute
    {
        protected bool enableRegen = true;
        /// <summary>
        /// Base regeneration value.
        /// </summary>
        protected float _baseRegen;
        protected float _currentRegen;
        /// <summary>
        /// Current regeneration value after multipliers.
        /// </summary>
        public float RegenValue { get => _currentRegen; }
        
        private float _regenMultiplier;
        /// <summary>
        /// Regeneration multiplier.
        /// </summary>
        protected float RegenMultiplier { get => _regenMultiplier; }

        /// <summary>
        /// Called every second.
        /// </summary>
        public virtual void Regen()
        {
            if (!enableRegen) return;

            _currentValue += RegenValue;
        }
        
        /// <summary>
        /// Set current regeneration value per second.
        /// </summary>
        protected void SetRegen(float value)
        {
            _currentRegen = value;
            ValueChange();
        }

        public void AddRegenMultiplier(float value)
        {
            _regenMultiplier += value;
            SetRegen(_baseRegen * _regenMultiplier);
        }

        public void SetRegenMultiplier(float value)
        {
            _regenMultiplier = value;
            SetRegen(_baseRegen * _regenMultiplier);
        }
    }
}
