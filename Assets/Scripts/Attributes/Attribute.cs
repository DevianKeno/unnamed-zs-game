using System;

using UnityEngine;

using UZSG.Systems;
using UZSG.Data;

namespace UZSG.Attributes
{
    public enum Type { Generic, Vital }
    public enum Change { Static, Regen, Degen }
    public enum Cycle { PerSecond, PerTick }
    
    /// <summary>
    /// Base class for Attributes.
    /// </summary>
    [Serializable]
    public class Attribute
    {
        public enum ValueChangeType { Increased, Decreased }

        public struct ValueChangedInfo
        {
            /// <summary>
            /// The value before the change.
            /// </summary>
            public float Previous { get; set; }
            /// <summary>
            /// The value after the change.
            /// </summary>
            public float New { get; set; }
            /// <summary>
            /// The amount of value changed.
            /// </summary>
            public float Change { get; set; }
            public bool IsBuffered { get; set; }
            public ValueChangeType ValueChangeType { get; set; }
        }

        public static Attribute None => null;
        [SerializeField] protected AttributeData data;
        public AttributeData Data => data;
        [SerializeField] protected float _value;
        /// <summary>
        /// Represents the current value.
        /// </summary>
        public float Value => _value;
        /// <summary>
        /// Returns a value between 0 and 1, representing the value to max ratio.
        /// </summary>
        public float ValueMaxRatio
        {
            get { return Value / Maximum; }
        }
        /// <summary>
        /// Represents the base maximum value, without any multipliers.
        /// </summary>
        public float BaseMaximum;
        /// <summary>
        /// Flat value added to the base maximum value, before multipliers.
        /// </summary>
        public float Bonus;
        /// <summary>
        /// Represents the current maximum value, after multipliers have been applied.
        /// Maximum = BaseMax * Multiplier
        /// </summary>
        public float Maximum
        {
            get
            {
                return (BaseMaximum + Bonus) * _multiplier;
            }
        }
        /// <summary>
        /// Multiplier for the BaseMax. Default is 1.
        /// Maximum = (BaseMax * Multiplier)
        /// </summary>
        [SerializeField] float _multiplier = 1;
        public float Multiplier
        {
            get => _multiplier;
            set
            {
                if (value > 0)
                {
                    _multiplier = value;
                }else
                {
                    Game.Console?.Log($"Cannot set Multiplier for Attribute {Data.Name}. A negative multiplier is invalid.");
                }
            }
        }
        
        protected float _minimum = 0f;
        public float Minimum => _minimum;
        public bool LimitOverflow = true;
        public bool LimitUnderflow = true;
        protected float _previousValue;
        public bool IsValid
        {
            get
            {
                return data != null;
            }
        }


        #region Events        

        /// <summary>
        /// Fired everytime ONLY IF the value of this attribute is changed.
        /// </summary>
        public event EventHandler<ValueChangedInfo> OnValueChanged;
        /// <summary>
        /// Called when the value reaches zero.
        /// </summary>
        public event EventHandler<ValueChangedInfo> OnReachZero;

        #endregion


        public Attribute(AttributeData data)
        {
            this.data = data;
        }

        internal virtual void Init() {}

        public static void ToMax(Attribute attr)
        {
            attr._previousValue = attr.Value;
            attr._value = attr.Maximum;            
            attr.ValueChanged();
        }
        
        public static void ToMin(Attribute attr)
        {
            attr._previousValue = attr.Value;
            attr._value = attr.Minimum;
            attr.ValueChanged();
        }
        
        public static void ToZero(Attribute attr)
        {
            attr._previousValue = attr.Value;
            attr._value = 0f;
            attr.ValueChanged();
        }

        /// <summary>
        /// Add amount to the attribute's value.
        /// </summary>
        public virtual void Add(float value)
        {
            _previousValue = Value;
            _value += value;
            if (_previousValue == Value) return;
            CheckOverflow();
            ValueChanged();
        }
        
        /// <summary>
        /// Remove amount from the attribute's value.
        /// </summary>
        public virtual void Remove(float value, bool buffer = false)
        {
            _previousValue = Value;
            _value -= value;
            if (_previousValue == Value) return;
            CheckUnderflow();
            ValueChanged(buffer);
        }
        
        /// <summary>
        /// Tries to remove the amount from the attribute's current value.
        /// Returns true if the value is less than the current value, false otherwise.
        /// </summary>
        public bool TryRemove(float value)
        {
            if (value < Value)
            {
                _previousValue = Value;
                _value -= value;
                CheckUnderflow();
                ValueChanged();
                return true;
            }
            return false;
        }
        
        protected virtual void ValueChanged(bool buffer = false)
        {
            if (Value == _previousValue) return;
            
            float value = Mathf.Abs(Value - _previousValue);
            OnValueChanged?.Invoke(this, new()
            {
                Previous = _previousValue,
                Change = value,
                New = Value,
                ValueChangeType = Value > _previousValue ? ValueChangeType.Increased : ValueChangeType.Decreased,
                IsBuffered = buffer,
            });

            if (Value <= 0)
            {
                OnReachZero?.Invoke(this, new()
                {
                    Previous = _previousValue,
                    Change = value,
                    New = Value,
                    ValueChangeType = Value > _previousValue ? ValueChangeType.Increased : ValueChangeType.Decreased,
                    IsBuffered = buffer,
                });
                return;
            }
        }

        protected float CheckOverflow()
        {
            if (!LimitOverflow) return 0f;

            float overflow = 0f;

            if (Value > Maximum)
            {
                overflow = Value - Maximum;
                _value -= overflow;
            }

            return overflow;
        }

        protected float CheckUnderflow()
        {
            if (!LimitUnderflow) return 0f;

            float underflow = 0f;

            if (Value < Minimum)
            {
                underflow = Value;
                _value += Mathf.Abs(underflow);
            }

            return underflow;
        }
    }
}
