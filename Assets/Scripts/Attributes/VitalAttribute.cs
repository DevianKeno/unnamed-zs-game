using System;

using UnityEngine;

using UZSG.Systems;
using UZSG.Data;

namespace UZSG.Attributes
{
    [Serializable]
    public class VitalAttribute : Attribute
    {
        [SerializeField] protected bool allowChange = true;
        public bool AllowChange
        {
            get
            {
                return allowChange;
            }
            set
            {
                allowChange = value;
            }
        }

        [SerializeField, Tooltip("Whether the change in value is regeneration, degeneration, or static (no change).")]
        protected VitalAttributeChangeType changeType = VitalAttributeChangeType.Static;
        /// <summary>
        /// Whether the change in value is regeneration, degeneration, or static (no change).
        /// </summary>
        public VitalAttributeChangeType ChangeType
        {
            get
            {
                return changeType;
            }
            set
            {
                changeType = value;
            }
        }

        [SerializeField, Tooltip("Whether the change in value happens per second, or per tick.")]
        protected VitalAttributeTimeCycle timeCycle = VitalAttributeTimeCycle.Second;
        /// <summary>
        /// Whether the change in value happens per second, or per tick.
        /// </summary>
        public VitalAttributeTimeCycle TimeCycle
        {
            get
            {
                return timeCycle;
            }
            set
            {
                timeCycle = value;
            }
        }

        [SerializeField, Tooltip("The base change in value per cycle.")]
        protected float baseChange = 0f;
        /// <summary>
        /// The base change in value per cycle.
        /// </summary>
        public float BaseChange
        {
            get
            {
                return baseChange;
            }
            set
            {
                baseChange = value;
            }
        }

        [SerializeField, Tooltip("Multiplier for the change value. Default is 1. CurrentChange = (BaseChange * ChangeMultiplier)")]
        protected float changeMultiplier = 1f;
        /// <summary>
        /// Multiplier for the change value. Default is 1.
        /// CurrentChange = (BaseChange * ChangeMultiplier)
        /// </summary>
        public float ChangeMultiplier
        {
            get
            {
                return changeMultiplier;
            }
            set
            {
                changeMultiplier = Mathf.Clamp(value, 0, float.MaxValue);
            }
        }

        [SerializeField, Tooltip("Flat value added to the Base Change value, after multipliers. Can be negative.")]
        protected float changeFlatBonus = 0f;
        /// <summary>
        /// Flat value added to the Base Change value, after multipliers. Can be negative.
        /// </summary>
        public float ChangeFlatBonus
        {
            get
            {
                return changeFlatBonus;
            }
            set
            {
                changeFlatBonus = value;
            }
        }

        [SerializeField, Tooltip("Whether to delay the value's change when the value is modified.")]
        protected bool enableDelayedChange = false;
        /// <summary>
        /// Whether to delay the value's change when the value is modified.
        /// </summary>
        public bool EnableDelayedChange
        {
            get
            {
                return enableDelayedChange;
            }
            set
            {
                enableDelayedChange = value;
            }
        }

        [SerializeField, Tooltip("The delay time in seconds.")]
        protected float delayedChangeDuration = 0f;
        /// <summary>
        /// The delay time in seconds.
        /// </summary>
        public float DelayedChangeDuration
        {
            get
            {
                return delayedChangeDuration;
            }
            set
            {
                delayedChangeDuration = Mathf.Clamp(value, 0, float.MaxValue);
            }
        }

        /// <summary>
        /// Amount changed from the current Value per cycle, after multipliers.
        /// (BaseChange * _changeMultiplier) 
        /// </summary>
        public float CurrentChange
        {
            get
            {
                return baseChange * changeMultiplier;
            }
        }
     
        float _lockUntil;

        #region Events

        /// <summary>
        /// Called when the value reaches its current maximum value.
        /// </summary>
        public event EventHandler<AttributeValueChangedContext> OnReachMaximum;
        /// <summary>
        /// Called when the value reaches its minimum value.
        /// </summary>
        public event EventHandler<AttributeValueChangedContext> OnReachMinimum;

        #endregion
        

        public VitalAttribute(AttributeData data) : base(data)
        {
            this.data = data;
        }
        
        public VitalAttribute(string id) : base(id)
        {
            this.data = Game.Attributes.GetData(id);
        }

        internal override void Initialize()
        {
            base.Initialize();

            if (allowChange)
            {
                if (timeCycle == VitalAttributeTimeCycle.Tick)
                {
                    Game.Tick.OnTick += CycleTick;
                }
                else if (timeCycle == VitalAttributeTimeCycle.Second)
                {
                    Game.Tick.OnSecond += CycleSecond;
                }
            }
        }
        
        void CycleTick(TickInfo t)
        {
            PerformCycle();
        }

        void CycleSecond(SecondInfo s)
        {
            PerformCycle();
        }

        /// <summary>
        /// Change current value by the ChangeType.
        /// </summary>
        void PerformCycle()
        {
            if (!allowChange) return;

            if (changeType == VitalAttributeChangeType.Regen)
            {
                value += CurrentChange;
                CheckOverflow();
                
            }
            else if (changeType == VitalAttributeChangeType.Degen)
            {
                value -= CurrentChange;
                CheckUnderflow();
                
            }
            ValueChanged();
        }
        
        public override void Remove(float value)
        {
            base.Remove(value);

            if (enableDelayedChange)
            {
                allowChange = false;
                _lockUntil = Time.time + delayedChangeDuration;
                Game.Tick.OnTick += CheckForChange;
            }
        }

        void CheckForChange(TickInfo t)
        {
            if (Time.time >= _lockUntil)
            {
                allowChange = true;
                _lockUntil = -1f;
                Game.Tick.OnTick -= CheckForChange;
            }
        }

        protected override void ValueChanged()
        {
            base.ValueChanged();
                       
            float valueChange = Mathf.Abs(value - previousValue);
            AttributeValueChangedContext context = new()
            {
                Previous = previousValue,
                Change = valueChange,
                New = value
            };

            if (Value <= minimum)
            {
                OnReachMinimum?.Invoke(this, context);
            }
            else if (Value >= CurrentMaximum)
            {
                OnReachMaximum?.Invoke(this, context);
            }
        }
                
        public virtual void ReadSaveData(VitalAttributeSaveData data, bool initialize = true)
        {
            base.ReadSaveData(data, initialize: false);

            allowChange = data.AllowChange;
            changeType = data.ChangeType;
            timeCycle = data.TimeCycle;
            baseChange = data.BaseChange;
            changeMultiplier = data.ChangeMultiplier;
            changeFlatBonus = data.ChangeFlatBonus;
            enableDelayedChange = data.EnableDelayedChange;
            delayedChangeDuration = data.DelayedChangeDuration;

            if (initialize)
            {
                Initialize();
            }
        }
    }
}