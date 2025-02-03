using System;

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

        public event Action OnEventStarted;
        public event Action OnEventEnded;

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
            OnEventStarted?.Invoke();
        }

        public void EndEvent()
        {
            OnEnd();
            OnEventEnded?.Invoke();
        }

        public virtual void OnStart() { }
        public virtual void OnEnd() { }
    }
}