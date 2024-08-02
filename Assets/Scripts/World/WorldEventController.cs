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
    public class WorldEventsController : MonoBehaviour
    {
        public int InternalCountdown = 0;
        public List<WorldEvent> WorldEvents;
        public List<WorldEventData> worldEventsData;

        [SerializeField] int _countdown = 0;
        float _currentTime = 0;
        int _maxCountdown = 0;
        int tempCount = 0;

        TimeController _timeController;
        WeatherController _weatherController;

        public void Initialize()
        {
            _timeController = this.GetComponent<TimeController>();
            _weatherController = this.GetComponent<WeatherController>();

            _timeController.Initialize();
            _weatherController.Initialize();

            Game.Tick.OnTick += OnTick;
            
            // foreach (var worldEvent in WorldEvents)
            // {
            //     if (worldEvent.Data.OccurEverySecond > _maxCountdown) 
            //     {
            //         _maxCountdown = worldEvent.Data.OccurEverySecond;
            //     }
            // }
            
            foreach (var worldEventData in worldEventsData)
            {
                if (worldEventData.OccurEverySecond > _maxCountdown) 
                {
                    _maxCountdown = worldEventData.OccurEverySecond;
                }
            }
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
            StartEvent();
        }

        void StartEvent()
        {
            foreach (WorldEvent worldEvent in WorldEvents)
            {
                if (worldEvent.IsActive)
                {
                    SpawnEvent(worldEvent);
                }
            }   
        }

        void InitializeEvent(WorldEvent worldEvent)
        {
            // worldEvent.OnEventStart += _weatherController.OnEventStart;

            worldEvent.OnEventStart += OnEventStart;
            worldEvent.OnEventStart += _timeController.OnEventStart;
            worldEvent.OnEventStart += _weatherController.OnEventStart;
            
            worldEvent.OnEventEnd += OnEventEnd;
            worldEvent.OnEventEnd += _timeController.OnEventEnd;
            worldEvent.OnEventEnd += _weatherController.OnEventEnd;
            
            worldEvent.StartEvent();

        }

        void OnEventStart(object sender, string e)
        {
            ///
        }

        void OnEventEnd(object sender, string e)
        {
            /// 
        }

        void SpawnEvent(WorldEvent worldEvent)
        {
            if (worldEvent.IsActive) return;

            if (_countdown == worldEvent.Data.OccurEverySecond)
            {
                InitializeEvent(worldEvent);
   
                List<EventPrefab> eventPrefabs = new();
                EventPrefab selectedEvent;
                int chance = UnityEngine.Random.Range(1, 100);

                foreach (EventPrefab eventPrefab in worldEvent.Data.EventPrefab)
                {
                    if (eventPrefab.ChanceToOccur >= chance) eventPrefabs.Add(eventPrefab);
                }

                if (eventPrefabs.Count > 1)
                    selectedEvent = eventPrefabs[UnityEngine.Random.Range(0, eventPrefabs.Count)];
                else
                    selectedEvent = eventPrefabs[0];

                // worldEvent.EndEvent();
            }
        }
    }
}