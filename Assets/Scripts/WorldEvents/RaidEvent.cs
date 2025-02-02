using System;
using System.Collections.Generic;

using UnityEngine;

using UZSG.Data;
using UZSG.Entities;
using UZSG.Systems;

namespace UZSG.Worlds.Events
{
    public class RaidEvent : WorldEventBase
    {
        public event Action<RaidEvent> OnEndEvent;
        public bool allDead;
        HordeFormations hordeFormations;
        public HordeFormations HordeFormations 
        {
            get => hordeFormations;
            set => hordeFormations = value;
        }
        [SerializeField] List<Enemy> hordeZombies;

        RaidInstance _raidInstance;

        public RaidEvent(float duration) : base(duration)
        {
        }

        public RaidInstance RaidInstance
        {
            get => _raidInstance;
            set => _raidInstance = value;
        }
        public int RemainingZombies => hordeZombies.Count;

        bool EventOngoing;
        RaidEventType _raidType;
        RaidFormation _raidFormation;
        [SerializeField] GameObject raidEventPrefab;


        /// TEMPORARY VALUES THAT WILL BE REPLACED WHEN A MORE SOPHISTICATED SYSTEM IS IMPLEMENTED
        public int worldDifficulty = 1;
        public int numberOfPlayers = 1;
        public float difficultyMultiplier = 0.3f;
        public int setEnemiesPerPlayer = 3;

        void OnRaidEnd(RaidEvent raidInstance)
        {
            bool allDead = raidInstance.allDead;

            if (allDead)
            {
                Game.Console.LogInfo($"<color=#ad0909>Raid event ended. All enemies were slain</color>");
            }
            else
            {
                Game.Console.LogInfo($"<color=#ad0909>Raid event ended. Not all enemies were slain</color>");
            }

            raidInstance.OnEndEvent -= OnRaidEnd;
            // Destroy(raidInstance.transform.gameObject);

            EventOngoing = false;
        }

        Player GetRandomPlayer()
        {
            var index = UnityEngine.Random.Range(0, Game.World.CurrentWorld.Players.Count - 1);
            return Game.World.CurrentWorld.Players[index];
        }

        void SpawnHorde(Player selectedPlayer)
        {
            if (selectedPlayer == null) return;

            RaidInstance newRaidInstance = DetermineRaid();

            HordeFormations hordeFormation = new();
            {
                hordeFormation.HandlePrerequisites(newRaidInstance, selectedPlayer);
            }
        }

        RaidInstance DetermineRaid()
        {
            float enemiesPerPlayer = numberOfPlayers * setEnemiesPerPlayer;
            float enemyMultiplier = enemiesPerPlayer * (worldDifficulty * difficultyMultiplier);
            int totalEnemies = Mathf.FloorToInt(enemiesPerPlayer + enemyMultiplier);

            switch (UnityEngine.Random.Range(0, 2))
            {
                case 0:
                {
                    Game.Console.LogInfo($"<color=#ad0909>Spawning blob formation.</color>");
                    _raidFormation = RaidFormation.Blob;
                    break;
                }
                case 1:
                {
                    Game.Console.LogInfo($"<color=#ad0909>Spawning line formation.</color>");
                    _raidFormation = RaidFormation.Line;
                    break;
                }
            }

            RaidInstance newRaidInstance = new()
            {
                enemyId = _raidInstance.enemyId,
                raidType = _raidType,
                raidFormation = _raidFormation,
                mobCount = totalEnemies
            };

            return newRaidInstance;
        }

        public override void OnStart()
        {
            // if (worldEvent == null || EventOngoing)
            // {
            //     Game.Console.LogInfo($"<color=#ad0909>Event is null or ongoing.</color>");
            //     return;
            // }
            
            if (Game.World.CurrentWorld.Players.Count <= 0)
            {
                Game.Console.LogInfo($"<color=#ad0909>No players found.</color>");
                return;
            }

            _durationTimer = _raidInstance.mobCount * 10f;
            _durationTimer = Mathf.Clamp(_durationTimer, 60f, 600f);
            hordeZombies = HordeFormations.HordeZombies;

            Game.Console.LogInfo($"<color=#ad0909>Raid event started.</color>");
            EventOngoing = true;
            List<RaidEventInstance> selectedEvents = new();
            
            PerformRaidEventProcessing(selectedEvents);

            foreach (Enemy enemy in hordeZombies)
            {
                enemy.OnDeath += OnEntityKilled;
            }

            Game.Tick.OnTick += OnTick;
        }

        public override void OnEnd()
        {
            Game.Tick.OnTick -= OnTick;
            OnEndEvent?.Invoke(this);
        }

        void OnTick(TickInfo info)
        {
            float tickThreshold = Game.Tick.TPS / 64f;
            float secondsCalculation = Game.Tick.SecondsPerTick * (Game.Tick.CurrentTick / 32f) * tickThreshold;
            _durationTimer -= secondsCalculation;

            if(_durationTimer <= 0 && hordeZombies.Count > 0)
            {
                allDead = false;
                EndEvent();
            }
            else if(_durationTimer <= 0 && hordeZombies.Count == 0)
            {
                allDead = true;
                EndEvent();
            }
        }
        
        void OnEntityKilled(Enemy enemy)
        {
            if (hordeZombies.Contains(enemy))
            {
                enemy.OnDeath -= OnEntityKilled;
                hordeZombies.Remove(enemy);
            }
        }
        
        void PerformRaidEventProcessing(List<RaidEventInstance> selectedEvents)
        {
            foreach (RaidEventInstance raidEvent in selectedEvents)
            {
                _raidInstance.enemyId = raidEvent.EnemyData.Id;
                SpawnHorde(GetRandomPlayer());
                
                // if (raidInstance == null || raidInstance.HordeFormations == null || raidInstance.RemainingZombies == 0) continue;
            }
        }
    }
}