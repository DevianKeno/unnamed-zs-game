using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UZSG.Systems;
using UZSG.Timebase;
using UZSG.World.Weather;
using UZSG.WorldBuilder;


namespace UZSG.World
{
    public class WorldEventController : MonoBehaviour
    {
        TimeController _timeController;
        WeatherController _weatherController;
        float _currentTime = 0;
        public int InternalCountdown = 0;
        [SerializeField] int _countdown = 0;
        int _maxCountdown = 0;
        int tempCount = 0;
        public List<WorldEventData> WorldEvents;

        public void Initialize()
        {
            _timeController = this.GetComponent<TimeController>();
            _weatherController = this.GetComponent<WeatherController>();

            _timeController.Initialize();
            _weatherController.Initialize();

            Game.Tick.OnTick += OnTick;
            
            foreach (WorldEventData data in WorldEvents)
                if (data.worldEvents.OccurEverySecond > _maxCountdown)
                    _maxCountdown = data.worldEvents.OccurEverySecond;
        }

        void OnTick(TickInfo info)
        {
            float tickThreshold = Game.Tick.TPS / 64f;
            float secondsCalculation = Game.Tick.SecondsPerTick * (Game.Tick.CurrentTick / 32f) * tickThreshold;
            _currentTime += secondsCalculation;

            _timeController.OnTick(secondsCalculation);
            _weatherController.OnTick(secondsCalculation);
            
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
            EventPrefab? selectedEvent = SelectEvent(properties);
            if (selectedEvent == null) return;

            WorldEvent worldEvent = new()
            {
                EventInfo = properties,
                SelectedEvent = selectedEvent.Value
            };

            InitializeControllers(properties, worldEvent);
            worldEvent.Initialize();
        }

        EventPrefab? SelectEvent(WorldEventProperties properties)
        {
            if (_countdown != properties.OccurEverySecond) return null;
            
            EventPrefab selectedEvent;

            if(properties.ChanceToOccur < UnityEngine.Random.Range(1, 100))
            {
                Game.Console.Log($"<color=#34d5eb>Event did not occur.</color>");
                // Debug.Log("Event did not occur.");
                return null;
            }

            // print("Event of type " + properties.Type + " occurred.");
            Game.Console.Log($"<color=#34d5eb>Event of type {properties.Type} occured.</color>");


            int chance = UnityEngine.Random.Range(1, 100);
            List<EventPrefab> eventPrefabs = new();

            foreach (EventPrefab eventPrefab in properties.EventPrefab)
            {
                if (eventPrefab.ChanceToOccur >= chance) eventPrefabs.Add(eventPrefab);
            }

            if (eventPrefabs.Count > 1)
                selectedEvent = eventPrefabs[UnityEngine.Random.Range(0, eventPrefabs.Count)];
            else if (eventPrefabs.Count == 1)
                selectedEvent = eventPrefabs[0];
            else
            {
                // print("No event prefab selected.");
                Game.Console.Log($"<color=#e8eb34>No event prefab selected.</color>");
                return null;
            }
            Game.Console.Log($"<color=#e8eb34>Event occured: {selectedEvent.Name}</color>");
            // print("Event occurred: " + selectedEvent.Name);
            return selectedEvent;
        }

        void InitializeControllers(WorldEventProperties properties, WorldEvent eventHandler)
        {
            if (properties.Type == WorldEventType.Weather)
                eventHandler.OnSpawnEvent += _weatherController.OnEventStart;
        }
    }
}