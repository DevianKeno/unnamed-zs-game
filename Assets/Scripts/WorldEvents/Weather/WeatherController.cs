using System;
using System.Collections;
using System.Collections.Generic;

using UnityEditor.Rendering;
using UnityEngine;

using UZSG.Systems;
using UZSG.Data;
using UZSG.Worlds;

namespace UZSG.WorldEvents.Weather
{
    public class WeatherController : EventBehaviour
    {
        public bool InstantiateWeatherInEditor;
        WeatherData _currentWeather;
        public WeatherData CurrentWeather;
        public WeatherData DefaultWeather;
        WeatherData _defaultWeather => DefaultWeather;
        public WorldTimeController WorldTime => Game.World.CurrentWorld.Time;
        float _weatherDuration;
        [SerializeField] float _weatherCountdown;

        ///TEMPORARY UNTIL WEATHER SYSTEM IS IMPLEMENTED
        public GameObject ParticleParent;
        ParticleSystem _currentParticleSystem;

        public void Initialize()
        {
            DeleteChildren(ParticleParent.transform);
            CurrentWeather = CurrentWeather == null ? DefaultWeather : CurrentWeather;
            _currentParticleSystem = CurrentWeather.particleSystem;
            SetWeather(CurrentWeather);
            
        }
        void OnValidate()
        {
            CurrentWeather = CurrentWeather == null ? DefaultWeather : CurrentWeather;
            _currentParticleSystem = CurrentWeather.particleSystem;
            
            if (InstantiateWeatherInEditor) SetWeather(CurrentWeather);
        }

        public void OnTick(float deltaTime)
        {
            if (_weatherCountdown >= 0 || _weatherCountdown != -1)
            {
                _weatherCountdown -= deltaTime;
                HandleChange();
            }
        }

        void HandleChange()
        {
            if (_weatherCountdown <= 0f || _weatherCountdown == -1f)
            {
                if (_currentWeather != _defaultWeather)
                {
                    EventOngoing = false;
                    SetWeather(_defaultWeather);
                }
            }
        }

        void FollowPlayer(ParticleSystem particle)
        {
            print("Following player");
            if (Camera.main.transform == null) return;

            if (ParticleParent.transform.parent != Camera.main.transform)
            {
                ParticleParent.transform.SetParent(Camera.main.transform);
            }
            
            ParticleSystem particleInstance = Instantiate(particle, ParticleParent.transform);
            
            Vector3 offset = new Vector3(0, 16, 0);
            particleInstance.transform.position = Camera.main.transform.position + offset;
        }

        void DeleteChildren(Transform parent, bool immediate = false)
        {
            if (ParticleParent.transform.childCount <= 0) return;

            for (int i = 0; i < parent.childCount; i++) Destroy(parent.GetChild(i).gameObject);
        }

        public void SetWeather(WeatherData weather)
        {
            _currentWeather = weather;
            CurrentWeather = weather;
            _weatherDuration = _currentWeather.weatherAttributes.DurationSeconds;
            _weatherCountdown = _weatherDuration;
            _currentParticleSystem = weather.particleSystem;
            
            DeleteChildren(ParticleParent.transform);

            ParticleSystem particle = _currentParticleSystem;
            FollowPlayer(particle);

            WorldTime.DayFogColor = Color.Lerp(WorldTime.DayFogColor, _currentWeather.weatherProperties.DayFogColor, 1f);
            WorldTime.NightFogColor = Color.Lerp(WorldTime.NightFogColor, _currentWeather.weatherProperties.NightFogColor, 1f);
            
            HandleChange();
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
            
            Game.Console.Log($"<color=#ad0909>Weather event started.</color>");
            EventPrefab selectedEvent = @event.SelectedEvent[0];
            EventOngoing = true;
            SetWeather(selectedEvent.Prefab.GetComponent<RainDataHolder>().WeatherData);
        }        
    }
}

