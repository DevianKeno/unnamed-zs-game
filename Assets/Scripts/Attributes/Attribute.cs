using System;

using UnityEngine;

using UZSG.Systems;
using UZSG.Data;

namespace UZSG.Attributes
{
    public enum AttributeType {
        Generic, Vital
    }
    
    public enum VitalAttributeChangeType {
        Static, Regen, Degen
    }

    public enum VitalAttributeTimeCycle {
        Second, Tick
    }
    
    /// <summary>
    /// Base class for Attributes.
    /// </summary>
    [Serializable]
    public class Attribute
    {
        public enum ValueChangedType {
            Increased, Decreased
        }

        public static Attribute None => new(data: null);
        
        [SerializeField] protected AttributeData data;
        public AttributeData Data => data;

        [SerializeField, Tooltip("The current value.")]
        protected float value = 0f;
        /// <summary>
        /// The current value.
        /// Setting this value directly will clamp it to Minimum and CurrentMaximum.
        /// If you want overflow, set it to false and use Add().
        /// </summary>
        public float Value
        {
            get
            {
                return value;
            }
            set
            {
                this.value = Mathf.Clamp(value, minimum, CurrentMaximum);
            }
        }
        
        protected float minimum = 0f;
        /// <summary>
        /// The base minimum value. You probably don't need to change this :)
        /// </summary>
        public float Minimum
        {
            get
            {
                return minimum;
            }
            set
            {
                minimum = value;
            }
        }

        [SerializeField, Tooltip("Represents the base maximum value, without any multipliers.")]
        protected float baseMaximum = 100f;
        /// <summary>
        /// Represents the base maximum value, without any multipliers. Defaults to 100, change if needed.
        /// </summary>
        public float BaseMaximum
        {
            get
            {
                return baseMaximum;
            }
        }

        [SerializeField, Tooltip("Multiplier for the Base Maximum value. Default is 1. CurrentMaximum = (BaseMaximum * Multiplier)")]
        protected float multiplier = 1f;
        /// <summary>
        /// Multiplier for the Base Maximum value. Default is 1.
        /// CurrentMaximum = (BaseMaximum * Multiplier)
        /// </summary>
        public float Multiplier
        {
            get
            {
                return multiplier;
            }
            set
            {
                multiplier = Mathf.Clamp(value, 0, float.MaxValue);
            }
        }

        [SerializeField, Tooltip("Flat value added to the Base Maximum value, before multipliers. Can be negative.")]
        protected float flatBonus = 0f;
        /// <summary>
        /// Flat value added to the Base Maximum value, after multipliers. Can be negative.
        /// </summary>
        public float FlatBonus
        {
            get
            {
                return flatBonus;
            }
            set
            {
                flatBonus = value;
            }
        }

        /// <summary>
        /// Represents the current maximum value, after multipliers have been applied.
        /// Maximum = BaseMax * Multiplier
        /// </summary>
        public float CurrentMaximum
        {
            get
            {
                return (baseMaximum * multiplier) + flatBonus;
            }
        }
        /// <summary>
        /// Returns a value between 0 and 1, representing the value to current max ratio.
        /// </summary>
        public float ValueMaxRatio
        {
            get
            {
                return Value / CurrentMaximum;
            }
        }

        public bool LimitOverflow = true;
        public bool LimitUnderflow = true;
        protected float previousValue;
        public bool IsValid => data != null;


        #region Events        

        /// <summary>
        /// Fired everytime ONLY IF the value of this attribute is CHANGED.
        /// Meaning that if the value is modified, but is still the same, it is not called.
        /// </summary>
        public event EventHandler<AttributeValueChangedContext> OnValueChanged;
        /// <summary>
        /// Called when the value reaches zero.
        /// </summary>
        public event EventHandler<AttributeValueChangedContext> OnReachZero;

        #endregion


        public Attribute(AttributeData data)
        {
            this.data = data;
        }
        
        public Attribute(string id)
        {
            this.data = Game.Attributes.GetData(id);
        }

        internal virtual void Initialize() { }

        /// <summary>
        /// Add amount to the attribute's value.
        /// </summary>
        public virtual void Add(float value)
        {
            previousValue = Value;
            this.value += value;
            if (previousValue == Value) return;
            CheckOverflow();
            ValueChanged();
        }
        
        /// <summary>
        /// Remove amount from the attribute's value.
        /// </summary>
        public virtual void Remove(float value)
        {
            previousValue = Value;
            this.value -= value;
            if (previousValue == Value) return;
            CheckUnderflow();
            ValueChanged();
        }
        
        /// <summary>
        /// Tries to remove an amount from the Attribute's current Value.
        /// Returns true if the amount can be removed, false otherwise.
        /// </summary>
        public virtual bool TryRemove(float amount)
        {
            if (amount <= Value)
            {
                Remove(amount);
                return true;
            }
            
            return false;
        }
        
        protected virtual void ValueChanged()
        {
            if (value == previousValue) return;
            
            float valueChange = Mathf.Abs(value - previousValue);
            AttributeValueChangedContext context = new()
            {
                Previous = previousValue,
                Change = valueChange,
                New = value
            };

            OnValueChanged?.Invoke(this, context);
            if (Value <= 0)
            {
                OnReachZero?.Invoke(this, context);
            }
        }

        protected float CheckOverflow()
        {
            if (!LimitOverflow) return 0f;

            float overflow = 0f;
            if (value > CurrentMaximum)
            {
                overflow = value - CurrentMaximum;
                value -= overflow;
            }

            return overflow;
        }

        protected float CheckUnderflow()
        {
            if (!LimitUnderflow) return 0f;

            float underflow = 0f;
            if (value < minimum)
            {
                underflow = value;
                value += Mathf.Abs(underflow); /// negative
            }

            return underflow;
        }
        
        public virtual void ReadSaveData(AttributeSaveData data, bool initialize = true)
        {
            value = data.Value;
            minimum = data.Minimum;
            baseMaximum = data.BaseMaximum;
            multiplier = data.Multiplier;
            flatBonus = data.FlatBonus;
            LimitOverflow = data.LimitOverflow;
            LimitUnderflow = data.LimitUnderflow;

            if (initialize)
            {
                Initialize();
            }
        }
        

        #region Static

        public static void ToMax(Attribute attr)
        {
            attr.previousValue = attr.Value;
            attr.value = attr.CurrentMaximum;
            attr.ValueChanged();
        }
        
        public static void ToMin(Attribute attr)
        {
            attr.previousValue = attr.Value;
            attr.value = attr.Minimum;
            attr.ValueChanged();
        }
        
        public static void ToZero(Attribute attr)
        {
            attr.previousValue = attr.Value;
            attr.value = 0f;
            attr.ValueChanged();
        }

        #endregion
    }
}
