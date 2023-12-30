using System;
using UnityEngine;
using UZSG.Systems;

namespace UZSG.Attributes
{
    /// <summary>
    /// A vital attribute represents a value between 0 and MaxValue.
    /// The value can either be static or regenerate/degenerate over time, which changes every second.
    /// </summary>
    [Serializable]
    public partial class VitalAttribute : Attribute
    {
        public static float Min => 0f;
        /// <summary>
        /// The natural change in value over time.
        /// </summary>
        public enum CycleType { Regen, Degen, Static }
        public CycleType Type;
        public enum CycleTime { Tick, Second }
        public CycleTime Time;
        protected bool _allowChange = true;
        protected bool _allowRegen = true;
        protected bool _allowDegen = true;
        protected bool _isEnabled = true;
        protected bool _limitOverflow = true;
        protected bool _limitUnderflow = true;
        protected float _minimum = 0f;
        protected float _delayedChangeDuration;
                
        [SerializeField] protected float _baseMaximum;
        /// <summary>
        /// Base maximum value.
        /// </summary>
        public float BaseMax => _baseMaximum;

        [SerializeField] protected float _currentMaximum;
        /// <summary>
        /// Current maximum value, after multipliers.
        /// </summary>
        public float Max => _currentMaximum;

        [SerializeField] protected float _multiplier = 1;
        /// <summary>
        /// Multiplier for BaseMax. Default is 1.
        /// Max = (BaseMax * Multiplier)
        /// </summary>
        public float Multiplier => _multiplier;

        [SerializeField] protected float _baseRegen;
        /// <summary>
        /// Base regeneration value.
        /// </summary>
        public float BaseRegen => _baseRegen;

        [SerializeField] protected float _currentRegen;
        /// <summary>
        /// Amount added to the current Value per cycle, after multipliers.
        /// (Value += RegenValue)
        /// </summary>
        public float RegenValue => _currentRegen;

        /// <summary>
        /// Base degeneration value.
        /// </summary>
        [SerializeField] protected float _baseDegen;
        /// <summary>
        /// Base degeneration value.
        /// </summary>
        public float BaseDegen => _baseDegen;
        
        [SerializeField] protected float _currentDegen;
        /// <summary>
        /// Amount removed from the current Value per cycle, after multipliers.
        /// (Value -= DegenValue) 
        /// </summary>
        public float DegenValue => _currentDegen;
        
        [SerializeField] protected float _regenMultiplier = 1;
        /// <summary>
        /// Regeneration value multiplier. Default is 1.
        /// (RegenValue = BaseRegen * RegenMultiplier)
        /// </summary>
        public float RegenMultiplier => _regenMultiplier;

        [SerializeField] protected float _degenMultiplier = 1;
        /// <summary>
        /// Degeneration value multiplier. Default is 1.
        /// (DegenValue = BaseDegen * DegenMultiplier)
        /// </summary>
        public float DegenMultiplier => _degenMultiplier;

        /// <summary>
        /// Called when the value reaches its maximum.
        /// </summary>
        public event Action OnReachFull;
        /// <summary>
        /// Called when the value reaches zero.
        /// </summary>
        public event Action OnReachZero;

        public VitalAttribute(float baseMax, float baseChange, CycleType cycle, CycleTime time) : base(baseMax)
        {
            _baseMaximum = baseMax;
            _value = baseMax;
            Type = cycle;
            Time = time;

            _currentMaximum = _baseMaximum;

            if (Type == CycleType.Regen)
            {
                _baseRegen = _currentRegen = baseChange;
            } else if (Type == CycleType.Degen)
            {
                _baseDegen = _currentDegen = baseChange;
            }

            if (Type != CycleType.Static)
            {
                if (Time == CycleTime.Tick)
                    Game.Tick.OnTick += Update;
                else                
                    Game.Tick.OnSecond += Update;
            }
        }

        ~VitalAttribute()
        {
            Game.Tick.OnTick -= Update;
            Game.Tick.OnSecond -= Update;
        }

        void Update(object sender, TickEventArgs e)
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

            if (_value < _currentMaximum)
                _value += RegenValue;
            else
                _value = _currentMaximum;
        }

        /// <summary>
        /// Decrease current value by current degeneration value.
        /// </summary>
        void Degen()
        {
            if (!_allowDegen) return;

            if (_value < _currentMaximum)
                _value -= DegenValue;
            else
                _value = _currentMaximum;
        }
        
        protected override void ValueChanged()
        {
            base.ValueChanged();

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
        void SetRegen(float value)
        {
            _currentRegen = value;
        }

        /// <summary>
        /// Set maximum value for attribute.
        /// </summary>
        void SetMax(float value)
        {
            _currentMaximum = value;
        }

        /// <summary>
        /// Start regeneration/degeneration cycle.
        /// </summary>
        /// <param name="cycle"></param>
        public void StartCycle(CycleType cycle)
        {
            _allowChange = true;

            if (cycle == CycleType.Regen)
            {
                Regen();
            } else if (cycle == CycleType.Degen)
            {
                Degen();
            }
        }

        /// <summary>
        /// Whether to allow the change of this value over time.
        /// </summary>
        public void AllowRegen(bool value)
        {
            _allowRegen = value;
        }

        /// <summary>
        /// Value += (BaseRegen * RegenMultipler) per cycle.
        /// </summary>
        public void AddRegenMultiplier(float value)
        {
            _regenMultiplier += value;
            SetRegen(_baseRegen * _regenMultiplier);
        }

        /// <summary>
        /// (RegenValue * RegenMultipler) per cycle.
        /// </summary>
        public void SetRegenMultiplier(float value)
        {
            _regenMultiplier = value;
            SetRegen(_baseRegen * _regenMultiplier);
        }

        public virtual float ToMax()
        {
            _value = _currentMaximum;
            ValueChanged();
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
