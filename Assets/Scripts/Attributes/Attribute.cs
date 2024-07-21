using System;
using UnityEngine;
using UZSG.Systems;

namespace UZSG.Attributes
{
    /// <summary>
    /// Base class for Attributes.
    /// </summary>
    [Serializable]
    public class Attribute
    {
        public enum ChangedType { Increased, Decreased }
        
        public struct ValueChangedArgs
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
            public ChangedType ChangedType { get; set; }
        }

        public static Attribute None => null;
        [SerializeField] protected AttributeData data;
        public AttributeData Data => data;
        /// <summary>
        /// Represents the current value.
        /// </summary>
        public float Value;
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
                } else
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
        public event EventHandler<ValueChangedArgs> OnValueChanged;
        /// <summary>
        /// Called when the value reaches zero.
        /// </summary>
        public event EventHandler<ValueChangedArgs> OnReachZero;

        #endregion


        public Attribute(AttributeData data)
        {
            this.data = data;
        }

        internal virtual void Init() {}

        public static void ToMax(Attribute attr)
        {
            attr._previousValue = attr.Value;
            attr.Value = attr.Maximum;            
            attr.ValueChanged();
        }
        
        public static void ToMin(Attribute attr)
        {
            attr._previousValue = attr.Value;
            attr.Value = attr.Minimum;
            attr.ValueChanged();
        }
        
        public static void ToZero(Attribute attr)
        {
            attr._previousValue = attr.Value;
            attr.Value = 0f;
            attr.ValueChanged();
        }

        /// <summary>
        /// Add amount to the attribute's value.
        /// </summary>
        public virtual void Add(float value)
        {
            _previousValue = Value;
            Value += value;
            if (_previousValue == Value) return;
            CheckOverflow();
            ValueChanged();
        }
        
        /// <summary>
        /// Remove amount from the attribute's value.
        /// </summary>
        public virtual void Remove(float value)
        {
            _previousValue = Value;
            Value -= value;
            if (_previousValue == Value) return;
            CheckUnderflow();
            ValueChanged();
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
                Value -= value;
                CheckUnderflow();
                ValueChanged();
                return true;
            }
            return false;
        }
        
        protected virtual void ValueChanged()
        {
            if (Value == _previousValue) return;
            
            float value = Mathf.Abs(Value - _previousValue);
            OnValueChanged?.Invoke(this, new()
            {
                Previous = _previousValue,
                Change = value,
                ChangedType = Value > _previousValue ? ChangedType.Increased : ChangedType.Decreased
            });

            if (Value <= 0)
            {
                OnReachZero?.Invoke(this, new()
                {
                    Previous = _previousValue,
                    Change = value,
                    New = Value
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
                Value -= overflow;
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
                Value += Mathf.Abs(underflow);
            }

            return underflow;
        }
    }
}
