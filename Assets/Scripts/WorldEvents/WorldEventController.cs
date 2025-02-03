using System.Collections.Generic;

using UnityEngine;

using UZSG.Systems;
using UZSG.Data;

namespace UZSG.Worlds.Events
{
    public class WorldEventController : MonoBehaviour//, ISaveDataReadWrite<WorldEventSaveData>
    {
        public World World { get; private set; }

        List<WorldEventBase> ongoingEvents = new();
        List<WorldEventBase> eventsToRemove = new();

        internal NaturalEnemySpawnEvent naturalEnemySpawnEvent;
        
        void Awake()
        {
            World = GetComponentInParent<World>();
        }

        public void Initialize()
        {
        }

        public void Deinitialize()
        {
            ongoingEvents.Clear();
            eventsToRemove.Clear();
        }

        internal void Tick(float deltaTime)
        {
            // foreach (WorldEventBase we in ongoingEvents)
            // {
            //     we.OnTick();
            //     we._durationTimer -= deltaTime;

            //     if (we._durationTimer < 0)
            //     {
            //         we.EndEvent();
            //         eventsToRemove.Add(we);
            //     }
            // }

            // if (Mathf.FloorToInt(_currentTime) > tempCount)
            // {
            //     tempCount = Mathf.FloorToInt(_currentTime);
            //     InternalClock++;
            //     if (_countdown >= _maxCountdown) 
            //         _countdown = 1;
            //     else 
            //         _countdown++;
                
            //     HandleEventTick();
            // }
        }

        void HandleEventTick()
        {
            // foreach (WorldEventData data in WorldEvents)
            // {
                // if (data.Enabled && _countdown % data.OccurEverySecond == 0)
                // {
                //     WorldEvent worldEvent = CreateEvent(data);
                    
                //     if (worldEvent == null) return;

                //     SubscribeControllers(data, worldEvent);
                //     worldEvent.SpawnEvent();
                // }
            // }
        }

        WorldEvent CreateEvent(WorldEventData eventData)
        {
            if(eventData.ChanceToOccur < UnityEngine.Random.Range(1, 100))
            {
                Game.Console.LogInfo($"<color=#34d5eb>Event did not occur.</color>");
                return null;
            }

            Game.Console.LogInfo($"<color=#34d5eb>Event of type {eventData.Type} occured.</color>");
            
            WorldEvent worldEvent = new()
            {
                EventData = eventData
            };
            var selectedEvent = worldEvent.PrepareEvent();
            if (selectedEvent == null) return null;

            return worldEvent;
        }

        void SubscribeControllers(WorldEventData eventData, WorldEvent eventHandler)
        {
            if (eventData.Type == WorldEventType.Weather)
            {
                // eventHandler.OnSpawnEvent += Weather.OnEventStart;
            }
            
            if (eventData.Type == WorldEventType.Raid)
            {
                // eventHandler.OnEventSpawned += Raid.OnEventStart;
            }
        }
        
        internal void BeginDefaultEvents()
        {
            /// This event is indefinite
            naturalEnemySpawnEvent = new NaturalEnemySpawnEvent();
            naturalEnemySpawnEvent.StartEvent();
        }
    }
}