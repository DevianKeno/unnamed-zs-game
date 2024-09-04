using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UZSG.Entities;
using UZSG.Interactions;
using UZSG.Systems;

namespace UZSG.Worlds.Events.Raid
{
    public class RaidInstanceHandler : MonoBehaviour
    {
        public event Action<RaidInstanceHandler> OnEndEvent;
        public bool allDead;
        HordeFormations hordeFormations;
        public HordeFormations HordeFormations 
        {
            get => hordeFormations;
            set => hordeFormations = value;
        }
        [SerializeField] List<IEnemy> _hordeZombies;

        RaidInstance _raidInstance;
        public RaidInstance RaidInstance
        {
            get => _raidInstance;
            set => _raidInstance = value;
        }

        public float remainingTime = 0;
        public int RemainingZombies => _hordeZombies.Count;

        public void Initialize()
        {
            Game.Tick.OnTick += OnTick;
            remainingTime = _raidInstance.mobCount * 10f;
            remainingTime = Mathf.Clamp(remainingTime, 60f, 600f);
            _hordeZombies = HordeFormations.HordeZombies;
            foreach (IEnemy enemy in _hordeZombies)
            {
                enemy.OnDeath += OnEntityKilled;
            }
        }
        void OnTick(TickInfo info)
        {
            float tickThreshold = Game.Tick.TPS / 64f;
            float secondsCalculation = Game.Tick.SecondsPerTick * (Game.Tick.CurrentTick / 32f) * tickThreshold;
            remainingTime -= secondsCalculation;

            if(remainingTime <= 0 && _hordeZombies.Count > 0)
            {
                allDead = false;
                EndEvent();
            }
            else if(remainingTime <= 0 && _hordeZombies.Count == 0)
            {
                allDead = true;
                EndEvent();
            }
        }
        void OnEntityKilled(IEnemy enemy)
        {
            if (_hordeZombies.Contains(enemy))
            {
                enemy.OnDeath -= OnEntityKilled;
                _hordeZombies.Remove(enemy);
            }
        }
        public void EndEvent()
        {
            OnEndEvent?.Invoke(this);
            Game.Tick.OnTick -= OnTick;
        }
    }
}