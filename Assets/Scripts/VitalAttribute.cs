using System;
using UnityEngine;
using URMG.Systems;

namespace URMG
{
    /// <summary>
    /// A vital attribute represents a value between 0 and a maximum value.
    /// This value can either be static or regenerate/degenerate over time.
    /// </summary>
    [Serializable]
    public class VitalAttribute : Attribute
    {
        /// <summary>
        /// The natural change in value over time.
        /// </summary>
        public enum Cycle { Regen, Degen, Static }

        protected Cycle _type;
        protected bool _allowChange = true;
        protected bool _allowRegen = true;
        protected bool _allowDegen = true;
        protected bool _isEnabled = true;
        protected bool _limitOverflow = true;
        protected bool _limitUnderflow = true;
        protected float _minimum = 0f;
                
        /// <summary>
        /// Base maximum value.
        /// </summary>
        
        [SerializeField] protected float _baseMaximum;
        public float BaseMaximum { get; }

        [SerializeField] protected float _currentMaximum;
        /// <summary>
        /// Current maximum value after multipliers.
        /// </summary>
        public float Max { get => _currentMaximum; }

        [SerializeField] protected float _multiplier;
        /// <summary>
        /// Multiplier for maximum value. 1 is default, representing the base maximum value.
        /// </summary>
        public float Multiplier { get => _multiplier; }

        /// <summary>
        /// Base regeneration value.
        /// </summary>
        [SerializeField] protected float _baseRegen;
        [SerializeField] protected float _currentRegen;
        /// <summary>
        /// Current regeneration value after multipliers.
        /// </summary>
        public float RegenValue { get => _currentRegen; }
        /// <summary>
        /// Base degeneration value.
        /// </summary>
        [SerializeField] protected float _baseDegen;
        [SerializeField] protected float _currentDegen;
        /// <summary>
        /// Current degeneration value after multipliers.
        /// </summary>
        public float DegenValue { get => _currentDegen; }
        
        private float _regenMultiplier;
        protected float RegenMultiplier { get => _regenMultiplier; }

        private float _degenMultiplier;
        protected float DegenMultiplier { get => _degenMultiplier; }

        public event Action OnReachFull;
        public event Action OnReachZero;

        public VitalAttribute(float baseMax, float value, Cycle cycle) : base(value)
        {
            _baseMaximum = baseMax;

            switch (cycle)
            {                    
                case Cycle.Regen:
                case Cycle.Degen:
                    Game.Tick.OnSecond += Second;
                    break;
            }
        }

        void Second(object sender, TickEventArgs e)
        {
            Degen();
            Regen();
        }

        /// <summary>
        /// Increase current value by current degeneration value.
        /// </summary>
        void Regen()
        {
            if (!_allowRegen) return;

            _value += RegenValue;
        }

        /// <summary>
        /// Decrease current value by current degeneration value.
        /// </summary>
        void Degen()
        {
            if (!_allowDegen) return;

            _value -= DegenValue;
        }
        
        protected override void OnValueChange()
        {
            base.OnValueChange();

            if (_value == _minimum)
            {
                OnReachZero?.Invoke();
                return;
            }

            if (_value == _currentMaximum)
            {
                OnReachFull?.Invoke();
                return;
            }
        }

        public override void Add(float value)
        {
            base.Add(value);
            CheckOverflow();
        }

        public override void Remove(float value)
        {
            base.Remove(value);
            CheckUnderflow();
        }

        protected float CheckOverflow()
        {
            if (!_limitOverflow) return 0f;

            float overflow = 0f;

            if (_value > _currentMaximum)
            {
                overflow = _value - _currentMaximum;
                _value -= overflow;
            }

            return overflow;
        }

        protected float CheckUnderflow()
        {
            if (!_limitUnderflow) return 0f;

            float underflow = 0f;

            if (_value < _minimum)
            {
                underflow = _value;
                _value += Mathf.Abs(underflow);
            }

            return underflow;
        }
        
        /// <summary>
        /// Set current regeneration value per second.
        /// </summary>
        protected void SetRegen(float value)
        {
            _currentRegen = value;
        }

        /// <summary>
        /// Set maximum value for attribute.
        /// </summary>
        protected void SetMax(float value)
        {
            _currentMaximum = value;
        }

        /// <summary>
        /// Start regeneration/degeneration cycle.
        /// </summary>
        /// <param name="cycle"></param>
        public void StartCycle(Cycle cycle)
        {
            if (!_allowChange) return;

            if (cycle == Cycle.Regen)
            {
                Regen();
            } else if (cycle == Cycle.Degen)
            {
                Degen();
            }
        }

        public void AllowRegen(bool value)
        {
            _allowRegen = value;
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

        public virtual float ToMax()
        {
            _value = _currentMaximum;
            OnValueChange();
            return _value;
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
    }
}
