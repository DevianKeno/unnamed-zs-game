using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

using UZSG.Systems;
using UZSG.Data;
using UZSG.Worlds;
using UZSG.WorldEvents.Weather;
using UZSG.WorldEvents.Raid;
using System.Reflection;

namespace UZSG.WorldEvents
{
    public class WorldEventController : MonoBehaviour
    {
        public WorldTimeController WorldTime => Game.World.CurrentWorld.Time;

        [SerializeField] WeatherController weatherController;
        public WeatherController Weather => weatherController;

        [SerializeField] RaidController raidController;
        public RaidController Raid => raidController;
        
        float _currentTime = 0;
        public int InternalClock = 0;
        [SerializeField] int _countdown = 0;
        int _maxCountdown = 0;
        int tempCount = 0;
        public List<WorldEventData> WorldEvents;

        public void Initialize()
        {
            InitializeControllers();
            Game.Tick.OnTick += OnTick;
            
            foreach (WorldEventData data in WorldEvents)
                if (data.OccurEverySecond > _maxCountdown)
                    _maxCountdown = data.OccurEverySecond;
        }

        void InitializeControllers()
        {
            weatherController.Initialize();
            // raidController.Initialize();
        }

        void OnTick(TickInfo info)
        {
            float tickThreshold = Game.Tick.TPS / 64f;
            float secondsCalculation = Game.Tick.SecondsPerTick * (Game.Tick.CurrentTick / 32f) * tickThreshold;
            _currentTime += secondsCalculation;

            WorldTime.OnTick(secondsCalculation);
            Weather.OnTick(secondsCalculation);
            // raidController.OnTick(secondsCalculation);

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
                Game.Console.Log($"<color=#34d5eb>Event did not occur.</color>");
                return null;
            }

            Game.Console.Log($"<color=#34d5eb>Event of type {eventData.Type} occured.</color>");
            
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
                eventHandler.OnSpawnEvent += Weather.OnEventStart;
            
            if (eventData.Type == WorldEventType.Raid)
                eventHandler.OnSpawnEvent += Raid.OnEventStart;
        }
    }
}