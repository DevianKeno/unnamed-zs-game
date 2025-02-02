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
        public bool allDead;
        HordeFormations hordeFormations;
        public HordeFormations HordeFormations 
        {
            get => hordeFormations;
            set => hordeFormations = value;
        }
        RaidInstance raidInstance;
        public RaidInstance RaidInstance
        {
            get => raidInstance;
            set => raidInstance = value;
        }
        
        [SerializeField] List<Enemy> hordeZombies;
        public int RemainingZombies => hordeZombies.Count;

        bool isOngoing;
        RaidEventType raidType;
        RaidFormation raidFormation;

        [SerializeField] GameObject raidEventPrefab;

        /// TEMPORARY VALUES THAT WILL BE REPLACED WHEN A MORE SOPHISTICATED SYSTEM IS IMPLEMENTED
        public float difficultyMultiplier = 0.3f;
        public int setEnemiesPerPlayer = 3;

        public RaidEvent(float duration) : base(duration)
        {
        }

        public override void OnStart()
        {
            if (Game.World.CurrentWorld.Players.Count <= 0)
            {
                Game.Console.LogInfo($"<color=#ad0909>No players found.</color>");
                return;
            }

            _durationTimer = raidInstance.MobCount * 10f;
            _durationTimer = Mathf.Clamp(_durationTimer, 60f, 600f);
            hordeZombies = HordeFormations.HordeZombies;

            Game.Console.LogInfo($"<color=#ad0909>Raid event started.</color>");
            isOngoing = true;
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
            
            if (allDead)
            {
                Game.Console.LogInfo($"<color=#ad0909>Raid event ended. All enemies were slain</color>");
            }
            else
            {
                Game.Console.LogInfo($"<color=#ad0909>Raid event ended. Not all enemies were slain</color>");
            }

            // Destroy(raidInstance.transform.gameObject);

            isOngoing = false;
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
                raidInstance.EnemyId = raidEvent.EnemyData.Id;
                SpawnHorde(GetRandomPlayer());
                
                // if (raidInstance == null || raidInstance.HordeFormations == null || raidInstance.RemainingZombies == 0) continue;
            }
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
            var currentWorld = Game.World.GetWorld();
            float enemiesPerPlayer = currentWorld.PlayerCount * setEnemiesPerPlayer;
            float enemyMultiplier = enemiesPerPlayer * (currentWorld.Attributes.DifficultyLevel * difficultyMultiplier);
            int totalEnemies = Mathf.FloorToInt(enemiesPerPlayer + enemyMultiplier);

            var formation = UnityEngine.Random.Range(0, 2);
            switch (formation)
            {
                case 0:
                {
                    Game.Console.LogInfo($"<color=#ad0909>Spawning blob formation.</color>");
                    raidFormation = RaidFormation.Blob;
                    break;
                }
                case 1:
                {
                    Game.Console.LogInfo($"<color=#ad0909>Spawning line formation.</color>");
                    raidFormation = RaidFormation.Line;
                    break;
                }
            }

            return raidInstance;
        }

    }
}