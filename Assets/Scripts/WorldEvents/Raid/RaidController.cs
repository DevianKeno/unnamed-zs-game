using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UZSG.Systems;
using UZSG.WorldEvents;



namespace UZSG.WorldEvents.Raid
{
    public class RaidController : EventBehaviour
    {
        public RaidEventType RaidType;
        [SerializeField] float _raidRemainingTime;

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

        void SpawnEnemey()
        {
            throw new System.NotImplementedException();
        }

        void RewardPlayers()
        {
            throw new System.NotImplementedException();
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
            EventPrefab selectedEvent = @event.SelectedEvent;
            EventOngoing = true;
        }
    }
}