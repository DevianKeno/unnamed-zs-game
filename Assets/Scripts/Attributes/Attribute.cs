using System;

using UnityEngine;

using UZSG.Data;
using UZSG.Saves;

namespace UZSG.Attributes
{
    /// <summary>
    /// Base class for Attributes.
    /// </summary>
    [Serializable]
    public class Attribute : ISaveDataReadWrite<AttributeSaveData>
    {
        public enum ValueChangeType {
            Increased, Decreased
        }

        public static Attribute None => new(data: null);
        
        [SerializeField] protected AttributeData data;
        public AttributeData Data => data;
        public string Id => data.Id;
        protected AttributeBroker broker;
        public AttributeBroker Broker => broker;
        
        public Attribute(AttributeData data)
        {
            this.data = data;
        }
        
        public Attribute(string id)
        {
            this.data = Game.Attributes.GetData(id);
            return;
        }

        public Attribute(Attribute attr)
        {
            data = attr.Data;
            value = attr.value;
            minimum = attr.Minimum;
            baseMaximum = attr.BaseMaximum;
            multiplier = attr.Multiplier;
            flatBonus = attr.FlatBonus;
            LimitOverflow = attr.LimitOverflow;
            LimitUnderflow = attr.LimitUnderflow;
        }
        
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
                // if (Broker.HasStatusEffects)
                // {
                //     return Broker.GetValue();
                // }
                // else
                // {
                    return value;
                // }
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
                if (CurrentMaximum <= 0)
                {
                    return 1f;
                }
                
                return Value / CurrentMaximum;
            }
        }
        public bool IsFull
        {
            get
            {
                return Value >= CurrentMaximum;
            }
        }
        public bool IsValid => data != null;

        [HideInInspector] public bool LimitOverflow = true;
        [HideInInspector] public bool LimitUnderflow = true;
        protected float previousValue;


        #region Events        

        /// <summary>
        /// Raised everytime ONLY IF the value of this attribute is CHANGED.
        /// Meaning that if the value is modified, but is still the same, it is not raised.
        /// </summary>
        public event Action<Attribute, AttributeValueChangedContext> OnValueChanged;
        /// <summary>
        /// Raised everytime the Add() or Remove() methods are raised.
        /// </summary>
        public event Action<Attribute, AttributeValueChangedContext> OnValueModified;
        /// <summary>
        /// Raised once everytime the value reaches its current maximum value.
        /// </summary>
        public event Action<Attribute, AttributeValueChangedContext> OnReachMaximum;
        /// <summary>
        /// Raised once everytime the value reaches its minimum value.
        /// </summary>
        public event Action<Attribute, AttributeValueChangedContext> OnReachMinimum;
        /// <summary>
        /// Raised once everytime the value reaches or passes zero.
        /// </summary>
        public event Action<Attribute, AttributeValueChangedContext> OnReachZero;

        #endregion


        internal virtual void Initialize()
        {
            
        }

        protected void AddInternal(float value)
        {
            if (value == 0) return;

            previousValue = Value;
            this.value += value;
            CheckOverflow();

            ValueChanged();
        }

        protected void RemoveInternal(float value)
        {
            if (value == 0) return;

            previousValue = Value;
            this.value -= value;
            CheckUnderflow();

            ValueChanged();
        }

        /// <summary>
        /// Add amount to the attribute's value.
        /// </summary>
        public virtual void Add(float value)
        {
            AddInternal(value);
            ValueModified();
        }

        /// <summary>
        /// Remove amount from the attribute's value.
        /// </summary>
        public virtual void Remove(float value)
        {
            RemoveInternal(value);
            ValueModified();
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
        
        protected virtual bool ValueChanged()
        {
            float valueChange = value - previousValue;
            if (Mathf.Abs(valueChange) <= 0) return false;

            AttributeValueChangedContext context = new()
            {
                Previous = previousValue,
                New = value
            };

            OnValueChanged?.Invoke(this, context);
            
            if (Value <= minimum)
            {
                if (previousValue > Minimum)
                {
                    OnReachMinimum?.Invoke(this, context);
                }
            }
            else if (Value >= CurrentMaximum)
            {
                if (previousValue < CurrentMaximum)
                {
                    OnReachMaximum?.Invoke(this, context);
                }
            }

            /// Check for passing zero based on the direction of the change
            if (valueChange < 0 && Value <= 0)
            {
                /// If the value decreased and passed below or equal to zero
                if (previousValue > 0)
                {
                    OnReachZero?.Invoke(this, context);
                }
            }
            else if (valueChange > 0 && Value >= 0)
            {
                /// If the value increased and passed above or equal to zero
                if (previousValue < 0)
                {
                    OnReachZero?.Invoke(this, context);
                }
            }

            return true;
        }

        protected virtual void ValueModified()
        {
            var info = new AttributeValueChangedContext()
            {
                Previous = previousValue,
                New = value,
            };
            OnValueModified?.Invoke(this, info);
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

        public void ReadSaveData(AttributeSaveData saveData)
        {
            ReadSaveJson(saveData, initialize: true);
        }
        
        public virtual void ReadSaveJson(AttributeSaveData data, bool initialize = true)
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

        public virtual AttributeSaveData WriteSaveData()
        {
            var saveData = new AttributeSaveData()
            {
                Id = Id,
                Value = value,
                Minimum = Minimum,
                BaseMaximum = BaseMaximum,
                Multiplier = Multiplier,
                FlatBonus = FlatBonus,
                LimitOverflow = LimitOverflow,
                LimitUnderflow = LimitUnderflow,
            };

            return saveData;
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
