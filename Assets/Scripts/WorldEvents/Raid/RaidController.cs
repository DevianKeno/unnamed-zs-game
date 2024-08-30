using System;
using System.Collections;
using System.Collections.Generic;

using UnityEditor.Rendering;
using UnityEngine;

using UZSG.Systems;
using UZSG.Data;
using UZSG.Worlds;
using UZSG.Entities;
using Unity.VisualScripting;
using Microsoft.Win32;


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
        RaidInstance _raidInstance;
        [SerializeField] GameObject raidEventPrefab;


        /// TEMPORARY VALUES THAT WILL BE REPLACED WHEN A MORE SOPHISTICATED SYSTEM IS IMPLEMENTED
        public int worldDifficulty = 1;
        public int numberOfPlayers = 1;
        public float difficultyMultiplier = 0.3f;
        public int setEnemiesPerPlayer = 3;

        void PerformRaidEventProcessing(List<RaidEventInstance> selectedEvents)
        {
            foreach (RaidEventInstance raidEvent in selectedEvents)
            {
                _raidInstance.enemyId = raidEvent.EnemyData.Id;
                RaidInstanceHandler raidInstance = SpawnHorde(SelectPlayerToSpawnHordeOn());
                raidInstance.OnEndEvent += OnRaidEnd;
            }
        }

        private void OnRaidEnd(bool allDead)
        {
            if (allDead)
            {
                Game.Console.Log($"<color=#ad0909>Raid event ended. All enemies were slain</color>");
            }
            else
            {
                Game.Console.Log($"<color=#ad0909>Raid event ended. Not all enemies were slain</color>");
            }

            EventOngoing = false;
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

        RaidInstanceHandler SpawnHorde(Player selectedPlayer)
        {
            RaidInstance newRaidInstance = DetermineRaid();

            HordeFormations hordeFormation = new();
            {
                hordeFormation.HandlePrerequisites(newRaidInstance, selectedPlayer);
            }

            var go = Instantiate(raidEventPrefab, transform);

            var rih = go.GetComponent<RaidInstanceHandler>();
            rih.HordeFormations = hordeFormation;
            rih.RaidInstance = newRaidInstance;

            rih.Initialize();

            return rih;
        }

        RaidInstance DetermineRaid()
        {
            float enemiesPerPlayer = numberOfPlayers * setEnemiesPerPlayer;
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
            
            if(Game.World.CurrentWorld.GetEntitiesById("player").Count == 0)
            {
                Game.Console.Log($"<color=#ad0909>No player found.</color>");
                return;
            }

            Game.Console.Log($"<color=#ad0909>Raid event started.</color>");
            EventOngoing = true;
            List<RaidEventInstance> selectedEvents = new();
            foreach (object selectedEvent in worldEvent.SelectedEvents)
                selectedEvents.Add((RaidEventInstance)selectedEvent);

            PerformRaidEventProcessing(selectedEvents);
        }
    }
}