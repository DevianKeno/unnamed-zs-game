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
    public class RaidController : EventBehaviour
    {
        public RaidEventType _raidType;
        [SerializeField] float _raidRemainingTime;
        [SerializeField] float _remainingMobs;

        public void Initialize()
        {

        }

        public void OnTick(float deltaTime)
        {
            if (_raidRemainingTime >= 0 || _raidRemainingTime != -1)
            {
                _raidRemainingTime -= deltaTime;
            }
        }

        void SpawnEnemy()
        {
            HordeFormations hordeFormations = new();
            {
                hordeFormations.SpawnFormation(_raidType, 10);
            }
        }

        void RewardPlayers()
        {
            throw new NotImplementedException();
        }

        public void OnEventStart(object sender, WorldEventProperties properties)
        {
            var @event = sender as WorldEvent;
            if (@event == null || EventOngoing)
            {
                Game.Console.Log($"<color=#ad0909>Event is null or ongoing.</color>");
                // print("Event is null or ongoing.");
                return;
            }

            Game.Console.Log($"<color=#ad0909>Raid event started.</color>");
            List<EventPrefab> selectedEvents = @event.SelectedEvent;
            EventOngoing = true;
        }
    }
}