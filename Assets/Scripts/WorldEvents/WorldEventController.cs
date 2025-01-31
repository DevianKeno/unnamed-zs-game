using System.Collections.Generic;

using UnityEngine;

using UZSG.Systems;
using UZSG.Data;
using UZSG.Worlds.Events.Raid;

namespace UZSG.Worlds.Events
{
    public class WorldEventController : MonoBehaviour
    {
        public World World { get; private set; }

        [SerializeField] RaidController raidController;
        public RaidController Raid => raidController;
        
        float _currentTime = 0;
        public int InternalClock = 0;
        [SerializeField] int _countdown = 0;
        int _maxCountdown = 0;
        int tempCount = 0;
        public List<WorldEventData> WorldEvents;

        void Awake()
        {
            World = GetComponentInParent<World>();
        }

        public void Initialize()
        {
            foreach (WorldEventData data in WorldEvents)
            {
                if (data.OccurEverySecond > _maxCountdown)
                {
                    _maxCountdown = data.OccurEverySecond;
                }
            }

            Game.Tick.OnTick += OnTick;
        }
        
        public void Deinitialize()
        {
            Game.Tick.OnTick -= OnTick;
        }

        void OnTick(TickInfo info)
        {
            if (Mathf.FloorToInt(_currentTime) > tempCount)
            {
                tempCount = Mathf.FloorToInt(_currentTime);
                InternalClock++;
                if (_countdown >= _maxCountdown) 
                    _countdown = 1;
                else 
                    _countdown++;
                
                HandleEvents();
            }
        }

        void HandleEvents()
        {
            foreach (WorldEventData data in WorldEvents)
                if (data.Enabled && _countdown % data.OccurEverySecond == 0)
                {
                    WorldEvent worldEvent = CreateEvent(data);
                    
                    if (worldEvent == null) return;

                    SubscribeControllers(data, worldEvent);
                    worldEvent.SpawnEvent();
                }
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
                eventHandler.OnEventSpawned += Raid.OnEventStart;
            }
        }
    }
}