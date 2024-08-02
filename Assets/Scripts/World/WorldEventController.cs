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
            
            foreach (WorldEvent worldEvent in WorldEvents.Select(worldEventData => worldEventData.worldEvents))
                if (worldEvent.OccurEverySecond > _maxCountdown) _maxCountdown = worldEvent.OccurEverySecond;
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
                if (_countdown > _maxCountdown) 
                    _countdown = 0;
                else 
                    _countdown++;
                
                HandleEvents();
            }
        }

        void HandleEvents()
        {
            foreach (WorldEvent worldEvent in WorldEvents.Select(worldEventData => worldEventData.worldEvents)) 
                if (worldEvent.Active) SpawnEvent(worldEvent);
        }

        void SpawnEvent(WorldEvent worldEvent)
        {
            if (worldEvent.EventOngoing) return;

            if (_countdown == worldEvent.OccurEverySecond)
            {
                worldEvent.OnEventStart += _weatherController.OnEventStart;
   
                List<EventPrefab> eventPrefabs = new();
                EventPrefab selectedEvent;
                int chance = UnityEngine.Random.Range(1, 100);

                foreach (EventPrefab eventPrefab in worldEvent.EventPrefab)
                {
                    if (eventPrefab.ChanceToOccur >= chance) eventPrefabs.Add(eventPrefab);
                }

                if (eventPrefabs.Count > 1)
                    selectedEvent = eventPrefabs[UnityEngine.Random.Range(0, eventPrefabs.Count)];
                else
                    selectedEvent = eventPrefabs[0];

                worldEvent.EventOngoing = true;
            }
        }
    }
}