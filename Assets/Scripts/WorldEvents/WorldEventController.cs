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
                if (data.worldEvents.OccurEverySecond > _maxCountdown)
                    _maxCountdown = data.worldEvents.OccurEverySecond;
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
                if (data.worldEvents.Active)
                {
                    print(data.worldEvents.Type);
                    SpawnEvent(data.worldEvents);
                }
        }

        void SpawnEvent(WorldEventProperties properties)
        {
            List<EventPrefab> selectedEvents = SelectEvent(properties);
            if (selectedEvents == null) return;

            WorldEvent worldEvent = new()
            {
                EventInfo = properties,
                SelectedEvent = selectedEvents
            };

            SubscribeControllers(properties, worldEvent);
            worldEvent.Initialize();
        }

        List<EventPrefab> SelectEvent(WorldEventProperties properties)
        {
            if (_countdown % properties.OccurEverySecond != 0) return null;
            
            List<EventPrefab> selectedEvents = new();

            if(properties.ChanceToOccur < UnityEngine.Random.Range(1, 100))
            {
                Game.Console.Log($"<color=#34d5eb>Event did not occur.</color>");
                return null;
            }

            Game.Console.Log($"<color=#34d5eb>Event of type {properties.Type} occured.</color>");

            int chance = UnityEngine.Random.Range(1, 100);
            foreach (EventPrefab eventPrefab in properties.EventPrefab)
            {
                if (eventPrefab.ChanceToOccur >= chance) selectedEvents.Add(eventPrefab);
            }
            
            if (selectedEvents.Count == 0)
            {
                Game.Console.Log($"<color=#e8eb34>No event prefab selected.</color>");
                return null;
            }
            else if (selectedEvents.Count > 1 && !properties.AllowMultipleEvents)
                selectedEvents = KeepOnlyAtIndex(selectedEvents, UnityEngine.Random.Range(0, selectedEvents.Count));
            
            foreach (EventPrefab eventPrefab in selectedEvents)
                Game.Console.Log($"<color=#e8eb34>Event occured: {eventPrefab.Name}</color>");
            
            return selectedEvents;
        }

        List<T> KeepOnlyAtIndex<T>(List<T> originalList, int index)
        {
            return new List<T> { originalList[index] };
        }

        void SubscribeControllers(WorldEventProperties properties, WorldEvent eventHandler)
        {
            if (properties.Type == WorldEventType.Weather)
                eventHandler.OnSpawnEvent += Weather.OnEventStart;
            
            if (properties.Type == WorldEventType.Raid)
                eventHandler.OnSpawnEvent += Weather.OnEventStart;
        }
    }
}