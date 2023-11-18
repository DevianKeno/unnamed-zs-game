
using System;
using UnityEngine;

namespace ZS
{
    /// <summary>
    /// An attribute represents a player stat.
    /// </summary>
    public abstract class Attribute
    {
        public event Action<Attribute> OnValueChanged;
        public event Action<Attribute> OnReachFull;
        public event Action<Attribute> OnReachZero;

        protected bool limitOverflow = true;
        protected bool isEnabled = true;
        protected float _minimum = 0f;

        /// <summary>
        /// Base maximum value.
        /// </summary>
        protected float _baseMaximum;
        public abstract float BaseMaximum { get; }

        protected float _currentMaximum;
        /// <summary>
        /// Current maximum value after multipliers.
        /// </summary>
        public float Max { get => _currentMaximum; }
    
        protected float _multiplier;
        /// <summary>
        /// Multiplier for maximum value. 1 is default, representing the base maximum value.
        /// </summary>
        public float Multiplier { get => _multiplier; }

        protected float _currentValue;
        /// <summary>
        /// Current attribute value.
        /// </summary>
        public float Value { get => _currentValue; }

        #region Methods

        protected void CheckOverflow()
        {
            if (!limitOverflow) return;

            float overflow;

            if (_currentValue > _currentMaximum)
            {
                overflow = _currentValue - _currentMaximum;
                _currentValue -= overflow;
            }
        }

        protected void CheckUnderflow()
        {
            float underflow;

            if (_currentValue < _minimum)
            {
                underflow = _currentValue;
                _currentValue += -underflow; // Needs checking
            }
        }

        protected void ValueChange()
        {
            OnValueChanged?.Invoke(this);

            if (_currentValue == _currentMaximum)
            {
                OnReachFull?.Invoke(this);
            } else if (_currentValue == _minimum)
            {
                OnReachZero?.Invoke(this);
            }
        }

        public virtual float AddAmount(float value)
        {
            _currentValue += value;
            CheckOverflow();
            ValueChange();
            return _currentValue;
        }

        public virtual float RemoveAmount(float value)
        {
            _currentValue -= value;
            CheckUnderflow();
            ValueChange();
            return _currentValue;
        }

        public virtual float ToFull()
        {
            _currentValue = _currentMaximum;
            ValueChange();
            return _currentValue;
        }

        /// <summary>
        /// Set maximum value for attribute.
        /// </summary>
        protected void SetMax(float value)
        {
            _currentMaximum = value;
        }

        public void AddMultiplier(float value)
        {
            _multiplier += value;
            SetMax(_baseMaximum * _multiplier);
        }

        public void SetMultiplier(float value)
        {
            _multiplier = value;
            SetMax(_baseMaximum * _multiplier);
        }

        #endregion
    }
}