using System;
using System.Collections.Generic;

using UZSG.Attributes;
using UZSG.Data;
using UZSG.Entities;


namespace UZSG.StatusEffects
{
    public abstract class StatusEffect
    {
        protected IAttributable actor;
        public IAttributable Actor => actor;
        protected StatusEffectData statusEffectData;
        public StatusEffectData Data => statusEffectData;

        public int Level;
        public float Duration;

        public event Action OnAfflicted;
        public event Action OnSecond;
        public event Action OnExpired;

        protected virtual void OnAfflict() { }
        protected virtual void Second() { }
        protected virtual void OnExpire() { }

        public void Expire()
        {
            Duration = 0;
            OnExpired?.Invoke();
        }
    }

    public class Slowed : StatusEffect
    {
        float _durationTimer;

        protected override void OnAfflict()
        {
            Game.Tick.OnTick += OnTick;
        }

        void OnTick(TickInfo info)
        {
            _durationTimer -= info.DeltaTime;
            if (_durationTimer <= 0)
            {
                Expire();
            }
        }
    }
}