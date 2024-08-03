using System;

using UnityEngine;

using UZSG.Systems;
using UZSG.Data;

namespace UZSG.Attributes
{
    [Serializable]
    public class VitalAttribute : Attribute
    {
        public bool AllowChange = true;
        public Change Change;
        public Cycle Cycle;
        /// <summary>
        /// Base change in value per cycle.
        /// </summary>
        public float BaseChange;    
        /// <summary>
        /// Change value multiplier. Default is 1.
        /// (CurrentChange = BaseChangeValue * ChangeValueMultiplier)
        /// </summary>
        [SerializeField] float _changeMultiplier = 1;
        public float ChangeMultiplier
        {
            get => _changeMultiplier;
            set
            {
                if (value > 0)
                {
                    _changeMultiplier = value;
                } else
                {
                    Game.Console?.Log($"Failed to set ChangeValueMultiplier for Attribute {Data.Id}. A negative multiplier is invalid.");
                }
            }
        }
        public bool DelayChange;
        public float DelayedChangeSeconds;
        /// <summary>
        /// Amount changed from the current Value per cycle, after multipliers.
        /// (BaseChange * _changeMultiplier) 
        /// </summary>
        public float CurrentChange
        {
            get
            {
                return BaseChange * _changeMultiplier;
            }
        }

        [SerializeField] float delayTimer;           

        #region Events
        /// <summary>
        /// Called when the value reaches its current maximum value.
        /// </summary>
        public event EventHandler<ValueChangedInfo> OnReachMaximum;
        /// <summary>
        /// Called when the value reaches its minimum value.
        /// </summary>
        public event EventHandler<ValueChangedInfo> OnReachMinimum;
        /// <summary>
        /// Called when the value reaches zero.
        /// </summary>
        #endregion
        
        public VitalAttribute(AttributeData data) : base(data)
        {
            this.data = data;
        }

        ~VitalAttribute()
        {
            Game.Tick.OnSecond -= CycleSecond;
            Game.Tick.OnTick -= CycleTick;
            Game.Tick.OnTick -= Tick;
        }

        internal override void Init()
        {
            if (Cycle == Cycle.PerSecond)
            {
                Game.Tick.OnSecond += CycleSecond;
            } else
            {                
                Game.Tick.OnTick += CycleTick;
            }
            
            Game.Tick.OnTick += Tick;
        }
        
        void Tick(TickInfo e)
        {
            delayTimer += Game.Tick.SecondsPerTick;
            
            if (delayTimer > DelayedChangeSeconds)
            {
                AllowChange = true;
            }
        }

        void CycleTick(TickInfo e)
        {
            PerformCycle();
        }

        void CycleSecond(SecondInfo e)
        {
            PerformCycle();
        }

        public override void Remove(float value, bool buffer = false)
        {
            base.Remove(value, buffer);

            if (DelayChange)
            {
                AllowChange = false;
                delayTimer = 0f;
            }
        }

        /// <summary>
        /// Change current value by the Change type.
        /// </summary>
        public void PerformCycle()
        {
            if (!AllowChange) return;

            if (Change == Change.Regen)
            {
                _value += BaseChange;
                CheckOverflow();
                
            } else if (Change == Change.Degen)
            {
                _value -= BaseChange;
                CheckUnderflow();
                
            }

            ValueChanged();
        }

        public void StartCycle()
        {
            if (Cycle == Cycle.PerSecond)
            {
                Game.Tick.OnTick += Tick;
            } else if (Cycle == Cycle.PerTick)
            {
                Game.Tick.OnSecond += CycleSecond;
            }
        }

        protected override void ValueChanged(bool buffer = false)
        {
            base.ValueChanged(buffer);
                       
            float value = Mathf.Abs(Value - _previousValue);
            if (Value <= _minimum)
            {
                OnReachMinimum?.Invoke(this, new()
                {
                    Previous = _previousValue,
                    Change = value,
                    New = Value,
                    ValueChangeType = Value > _previousValue ? ValueChangeType.Increased : ValueChangeType.Decreased,
                    IsBuffered = buffer,
                });
                return;
            }

            if (Value >= Maximum)
            {
                OnReachMaximum?.Invoke(this, new()
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
    }
}