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
            public float Previous;
            /// <summary>
            /// The value after the change.
            /// </summary>
            public float New;
            /// <summary>
            /// The amount of value changed.
            /// </summary>
            public float Change;
            public ChangedType ChangedType;

        }

        public static Attribute None => null;
        public AttributeData Data;
        /// <summary>
        /// Represents the current value.
        /// </summary>
        public float Value;
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
        
        public float Minimum = 0f;
        public bool LimitOverflow = true;
        public bool LimitUnderflow = true;
        protected float previousValue;

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
            Data = data;
        }

        ~Attribute()
        {
        }

        internal virtual void Initialize() {}

        public static void ToMax(Attribute attr)
        {
            attr.previousValue = attr.Value;
            attr.Value = attr.Maximum;            
            attr.ValueChanged();
        }
        
        public static void ToMin(Attribute attr)
        {
            attr.previousValue = attr.Value;
            attr.Value = attr.Minimum;
            attr.ValueChanged();
        }
        
        public static void ToZero(Attribute attr)
        {
            attr.previousValue = attr.Value;
            attr.Value = 0f;
            attr.ValueChanged();
        }

        /// <summary>
        /// Add amount to the attribute's value.
        /// </summary>
        public virtual void Add(float value)
        {
            previousValue = Value;
            Value += value;
            CheckOverflow();
            ValueChanged();
        }
        
        /// <summary>
        /// Remove amount from the attribute's value.
        /// </summary>
        public virtual void Remove(float value)
        {
            previousValue = Value;
            Value -= value;
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
                previousValue = Value;
                Value -= value;
                CheckUnderflow();
                ValueChanged();
                return true;
            }
            return false;
        }

        public void LoadValuesFromJSON(AttributeJSON data)
        {
            if (data == null) return;

            Value = data.Value;
        }
        
        protected virtual void ValueChanged()
        {
            if (Value == previousValue) return;
            
            float value = Mathf.Abs(Value - previousValue);
            OnValueChanged?.Invoke(this, new()
            {
                Previous = previousValue,
                Change = value,
                ChangedType = Value > previousValue ? ChangedType.Increased : ChangedType.Decreased
            });

            if (Value <= 0)
            {
                OnReachZero?.Invoke(this, new()
                {
                    Previous = previousValue,
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
