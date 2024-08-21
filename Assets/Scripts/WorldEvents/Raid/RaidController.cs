using System;
using System.Collections;
using System.Collections.Generic;

using UnityEditor.Rendering;
using UnityEngine;

using UZSG.Systems;
using UZSG.Data;
using UZSG.Worlds;


namespace UZSG.WorldEvents.Raid
{
    public struct RaidInstance
    {
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


        /// TEMPORARY VALUES THAT WILL BE REPLACED WHEN A MORE SOPHISTICATED SYSTEM IS IMPLEMENTED
        public int worldDifficulty = 1;
        public int numberOfPlayers = 1;
        public float difficultyMultiplier = 0.3f;

        void SpawnEnemy()
        {
            HordeFormations hordeInstance = new();
            {
                hordeInstance.SpawnFormation(CalculateRaidDifficulty());
            }
        }

        RaidInstance CalculateRaidDifficulty()
        {
            float enemiesPerPlayer = numberOfPlayers * 3f;
            float enemyMultiplier = enemiesPerPlayer * (worldDifficulty * difficultyMultiplier);
            int totalEnemies = Mathf.FloorToInt(enemiesPerPlayer + enemyMultiplier);

            switch (UnityEngine.Random.Range(0, 1))
            {
                case 0:
                    _raidFormation = RaidFormation.Blob;
                    break;
                case 1:
                    _raidFormation = RaidFormation.Line;
                    break;
            }

            RaidInstance newRaidInstance = new()
            {
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
            // List<EventPrefab> selectedEvents = worldEvent.SelectedEvent;
            // EventOngoing = true;
            // foreach (EventPrefab selectedEvent in selectedEvents)
            // {
                
            // }
        }
    }
}