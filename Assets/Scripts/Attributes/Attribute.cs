using System;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UZSG.Systems;

namespace UZSG.Attributes
{
    /// <summary>
    /// Represents a value of a stat.
    /// The value can either be static or regenerate/degenerate over time.
    /// </summary>
    [Serializable]
    public class Attribute
    {        
        public struct ValueChangedArgs
        {
            public float Previous;
            public float Change;
            public float New;
        }

        [SerializeField] AttributeData _data;
        public AttributeData Data => _data;
        /// <summary>
        /// Represents the current value.
        /// </summary>
        public float Value;
        public float PreviousValue;
        public float Minimum = 0f;
        /// <summary>
        /// Represents the base maximum value, without any multipliers.
        /// </summary>
        public float BaseMaximum;
        /// <summary>
        /// Represents the current maximum value, after multipliers have been applied.
        /// Maximum = (BaseMax * Multiplier)
        /// </summary>
        public float CurrentMaximum;
        /// <summary>
        /// Multiplier for the BaseMax. Default is 1.
        /// Maximum = (BaseMax * Multiplier)
        /// </summary>
        [SerializeField] float _baseMaxMultiplier = 1;
        public float BaseMaxMultiplier
        {
            get => _baseMaxMultiplier;
            set
            {
                if (value > 0)
                {
                    _baseMaxMultiplier = value;
                } else
                {
                    Game.Console?.Log($"Failed to set BaseMaxMultiplier for Attribute {_data.Id}. A negative multiplier is invalid.");
                }
            }
        }
        /// <summary>
        /// Base change in value.
        /// </summary>
        public float BaseChangeValue;        
        /// <summary>
        /// Amount removed from the current Value per cycle, after multipliers.
        /// (Value -= DegenerationValue) 
        /// </summary>
        public float CurrentChangeValue;        
        /// <summary>
        /// Change value multiplier. Default is 1.
        /// (CurrentChangeValue = BaseChangeValue * ChangeValueMultiplier)
        /// </summary>
        [SerializeField] float _changeValueMultiplier = 1;
        public float ChangeValueMultiplier
        {
            get => _changeValueMultiplier;
            set
            {
                if (value > 0)
                {
                    _changeValueMultiplier = value;
                } else
                {
                    Game.Console?.Log($"Failed to set ChangeValueMultiplier for Attribute {_data.Id}. A negative multiplier is invalid.");
                }
            }
        }
        public bool LimitOverflow = true;
        public bool LimitUnderflow = true;
        public bool AllowChange = true;
        public Change Change;
        public Cycle Cycle;
        public bool DelayedChange;
        public float DelayedChangeDuration;

        #region Events
        /// <summary>
        /// Fired everytime ONLY IF the value of this attribute is changed.
        /// </summary>
        public event EventHandler<ValueChangedArgs> OnValueChanged;
        /// <summary>
        /// Called when the value reaches its current maximum value.
        /// </summary>
        public event EventHandler<ValueChangedArgs> OnReachMaximum;
        /// <summary>
        /// Called when the value reaches its minimum value.
        /// </summary>
        public event EventHandler<ValueChangedArgs> OnReachMinimum;
        /// <summary>
        /// Called when the value reaches zero.
        /// </summary>
        public event EventHandler<ValueChangedArgs> OnReachZero;
        #endregion

        public Attribute(AttributeData data)
        {
            _data = data;
            Value = BaseMaximum;
        }

        ~Attribute()
        {
            Game.Tick.OnTick -= Update;
            Game.Tick.OnSecond -= Update;
        }

        public static void ToMax(Attribute attr)
        {
            attr.PreviousValue = attr.Value;
            attr.Value = attr.CurrentMaximum;            
            if (attr.PreviousValue != attr.Value) attr.ValueChanged(Mathf.Abs(attr.PreviousValue - attr.Value));
        }
        
        public static void ToMin(Attribute attr)
        {
            attr.PreviousValue = attr.Value;
            attr.Value = attr.Minimum;            
            if (attr.PreviousValue != attr.Value) attr.ValueChanged(Mathf.Abs(attr.PreviousValue - attr.Value));
        }
        
        public static void ToZero(Attribute attr)
        {
            attr.PreviousValue = attr.Value;
            attr.Value = 0f;            
            if (attr.PreviousValue != attr.Value) attr.ValueChanged(Mathf.Abs(attr.PreviousValue - attr.Value));
        }

        /// <summary>
        /// Add amount to the attribute's value.
        /// </summary>
        public void Add(float value)
        {
            PreviousValue = Value;
            Value += value;
            CheckOverflow();
            if (PreviousValue != Value) ValueChanged(Mathf.Abs(PreviousValue - Value));
        }
        
        /// <summary>
        /// Remove amount from the attribute's value.
        /// </summary>
        public void Remove(float value)
        {
            PreviousValue = Value;
            Value -= value;
            CheckUnderflow();
            if (PreviousValue != Value) ValueChanged(Mathf.Abs(PreviousValue - Value));
        }

        /// <summary>
        /// Change current value by the Change type.
        /// </summary>
        public void PerformChange()
        {
            if (!AllowChange) return;
            if (Change == Change.Static) return;

            if (Change == Change.Regen)
            {
                Value += BaseChangeValue;
            } else if (Change == Change.Degen)
            {
                Value -= BaseChangeValue;
            }
        }

        public void StartCycle()
        {
            if (Cycle == Cycle.PerSecond)
            {
                Game.Tick.OnTick += Update;
            } else if (Cycle == Cycle.PerTick)
            {
                Game.Tick.OnSecond += Update;
            }
        }

        void Update(object sender, TickEventArgs e)
        {
            PerformChange();
        }
        
        void ValueChanged(float value)
        {
            if (Value == Minimum)
            {
                OnReachMinimum?.Invoke(this, new()
                {
                    Previous = PreviousValue,
                    Change = value,
                });
                return;
            }

            if (Value == CurrentMaximum)
            {
                OnReachMaximum?.Invoke(this, new()
                {
                    Previous = PreviousValue,
                    Change = value,
                    New = Value
                });
                return;
            }

            if (Value <= 0)
            {
                OnReachZero?.Invoke(this, new()
                {
                    Previous = PreviousValue,
                    Change = value,
                    New = Value
                });
                return;
            }
        }

        float CheckOverflow()
        {
            if (!LimitOverflow) return 0f;

            float overflow = 0f;

            if (Value > CurrentMaximum)
            {
                overflow = Value - CurrentMaximum;
                Value -= overflow;
            }

            return overflow;
        }

        float CheckUnderflow()
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
