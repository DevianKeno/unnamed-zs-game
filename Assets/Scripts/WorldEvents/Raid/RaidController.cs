using System;
using System.Collections;
using System.Collections.Generic;

using UnityEditor.Rendering;
using UnityEngine;

using UZSG.Systems;
using UZSG.Data;
using UZSG.Worlds;
using UZSG.Entities;


namespace UZSG.WorldEvents.Raid
{
    public struct RaidInstance
    {
        public string enemyId; 
        public RaidEventType raidType;
        public RaidFormation raidFormation;
        public int mobCount;
    }

    public class RaidController : EventBehaviour
    {
        RaidEventType _raidType;
        RaidFormation _raidFormation;
        [SerializeField] float _raidRemainingTime;
        [SerializeField] float _remainingMobs;

        RaidInstance _raidInstance;


        /// TEMPORARY VALUES THAT WILL BE REPLACED WHEN A MORE SOPHISTICATED SYSTEM IS IMPLEMENTED
        public int worldDifficulty = 1;
        public int numberOfPlayers = 1;
        public float difficultyMultiplier = 0.3f;

        void PerformRaidEventProcessing(List<RaidEventInstance> selectedEvents)
        {
            foreach (RaidEventInstance raidEvent in selectedEvents)
            {
                _raidInstance.enemyId = raidEvent.EnemyData.Id;
                SpawnHorde(SelectPlayerToSpawnHordeOn());
            }
        }

        Player SelectPlayerToSpawnHordeOn()
        {
            var player = Game.World.CurrentWorld.GetEntitiesById("player");

            if (player.Count > 1)
            {
                return player[UnityEngine.Random.Range(0, player.Count)] as Player;
            }
            else if (player.Count == 1)
            {
                return player[0] as Player;
            }
            else
            {
                Game.Console.Log($"<color=#ad0909>No player found.</color>");
                return null;
            }
        }

        void SpawnHorde(Player selectedPlayer)
        {
            HordeFormations hordeFormation = new();
            {
                hordeFormation.HandlePrerequisites(DetermineRaid(), selectedPlayer);
            }

            RaidInstanceHandler raidInstance = new()
            {
                HordeFormations = hordeFormation
            };
        }

        RaidInstance DetermineRaid()
        {
            float enemiesPerPlayer = numberOfPlayers * 3f;
            float enemyMultiplier = enemiesPerPlayer * (worldDifficulty * difficultyMultiplier);
            int totalEnemies = Mathf.FloorToInt(enemiesPerPlayer + enemyMultiplier);

            switch (UnityEngine.Random.Range(0, 1))
            {
                case 0:
                    Game.Console.Log($"<color=#ad0909>Spawning blob formation.</color>");
                    _raidFormation = RaidFormation.Blob;
                    break;
                case 1:
                    Game.Console.Log($"<color=#ad0909>Spawning line formation.</color>");
                    _raidFormation = RaidFormation.Line;
                    break;
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

        public void OnEventStart(WorldEvent worldEvent)
        {
            if (worldEvent == null || EventOngoing)
            {
                Game.Console.Log($"<color=#ad0909>Event is null or ongoing.</color>");
                return;
            }

            Game.Console.Log($"<color=#ad0909>Raid event started.</color>");
            // EventOngoing = true;
            List<RaidEventInstance> selectedEvents = new();
            foreach (object selectedEvent in worldEvent.SelectedEvents)
                selectedEvents.Add((RaidEventInstance)selectedEvent);

            PerformRaidEventProcessing(selectedEvents);
        }
    }
}