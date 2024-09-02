using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UZSG.Entities;
using UZSG.Interactions;
using UZSG.Systems;

namespace UZSG.WorldEvents.Raid
{
    public class RaidInstanceHandler : MonoBehaviour
    {
        public event Action<bool> OnEndEvent;
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
            remainingTime = Mathf.Clamp(remainingTime, 60f, _raidInstance.mobCount * 10f);
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
                EndEvent(false);
            }
            else if(_hordeZombies.Count == 0)
            {
                EndEvent(true);
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
        public void EndEvent(bool allDead)
        {
            OnEndEvent?.Invoke(allDead);
            Destroy(this.transform.gameObject);
        }
    }
}