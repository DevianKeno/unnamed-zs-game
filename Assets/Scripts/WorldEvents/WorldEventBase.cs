using UnityEngine;

using UZSG.Data;

namespace UZSG.Worlds.Events
{
    public abstract class WorldEventBase
    {
        [SerializeField] protected WorldEventData eventData;
        public WorldEventData EventData => eventData;

        internal float _durationTimer;
        public float Duration
        {
            get => _durationTimer;
            set
            {
                SetDuration(value);
            }
        }

        public WorldEventBase(float duration)
        {
            SetDuration(duration);
        }

        public void SetDuration(float value)
        {
            if (value <= 0)
            {
                _durationTimer = 0;
                EndEvent();
            }
            else
            {
                _durationTimer = value;
            }
        }

        public void StartEvent()
        {
            OnStart();
        }

        public void EndEvent()
        {
            OnEnd();
        }

        public virtual void OnStart() { }
        public virtual void OnTick() { }
        public virtual void OnSecond() { }
        public virtual void OnEnd() { }
    }
}