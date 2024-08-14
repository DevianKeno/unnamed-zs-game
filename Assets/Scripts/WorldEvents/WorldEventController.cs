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
        WorldTimeController _timeController;
        WeatherController _weatherController;
        RaidController _raidController;
        float _currentTime = 0;
        public int InternalCountdown = 0;
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
            _timeController = this.GetComponent<WorldTimeController>();
            _weatherController = this.GetComponent<WeatherController>();
            _raidController = this.GetComponent<RaidController>();

            _timeController.Initialize();
            _weatherController.Initialize();
            _raidController.Initialize();
        }

        void OnTick(TickInfo info)
        {
            float tickThreshold = Game.Tick.TPS / 64f;
            float secondsCalculation = Game.Tick.SecondsPerTick * (Game.Tick.CurrentTick / 32f) * tickThreshold;
            _currentTime += secondsCalculation;

            _timeController.OnTick(secondsCalculation);
            _weatherController.OnTick(secondsCalculation);
            _raidController.OnTick(secondsCalculation);
            
            if (Mathf.FloorToInt(_currentTime) > tempCount)
            {
                tempCount = Mathf.FloorToInt(_currentTime);
                InternalCountdown++;
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
                    SpawnEvent(data.worldEvents);
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
            if (_countdown != properties.OccurEverySecond) return null;
            
            List<EventPrefab> selectedEvents = new();

            if(properties.ChanceToOccur < UnityEngine.Random.Range(1, 100))
            {
                Game.Console.Log($"<color=#34d5eb>Event did not occur.</color>");
                // Debug.Log("Event did not occur.");
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
                // print("No event prefab selected.");
                Game.Console.Log($"<color=#e8eb34>No event prefab selected.</color>");
                return null;
            }
            else if (selectedEvents.Count > 1 && !properties.AllowMultipleEvents)
                selectedEvents = KeepOnlyAtIndex(selectedEvents, UnityEngine.Random.Range(0, selectedEvents.Count));
            
            foreach (EventPrefab eventPrefab in selectedEvents)
                Game.Console.Log($"<color=#e8eb34>Event occured: {eventPrefab.Name}</color>");
            
            return selectedEvents;
        }

        public static List<T> KeepOnlyAtIndex<T>(List<T> originalList, int index)
        {
            return new List<T> { originalList[index] };
        }

        void SubscribeControllers(WorldEventProperties properties, WorldEvent eventHandler)
        {
            if (properties.Type == WorldEventType.Weather)
                eventHandler.OnSpawnEvent += _weatherController.OnEventStart;
            
            if (properties.Type == WorldEventType.Raid)
                eventHandler.OnSpawnEvent += _weatherController.OnEventStart;
        }
    }
}