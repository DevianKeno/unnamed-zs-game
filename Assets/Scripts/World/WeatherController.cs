using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;
using UZSG.Systems;
using UZSG.WorldBuilder;

namespace UZSG.World.Weather
{
    public class WeatherController : EventBehaviour
    {

        public bool InstantiateWeatherInEditor;
        WeatherData _currentWeather;
        public WeatherData CurrentWeather;
        public WeatherData DefaultWeather;
        WeatherData _defaultWeather => DefaultWeather;
        public TimeController Time;
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
            if (_weatherCountdown > 0 || _weatherCountdown != -1)
            {
                _weatherCountdown -= deltaTime;
            }

            HandleChange();
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

            Time.DayFogColor = Color.Lerp(Time.DayFogColor, _currentWeather.weatherProperties.DayFogColor, 1f);
            Time.NightFogColor = Color.Lerp(Time.NightFogColor, _currentWeather.weatherProperties.NightFogColor, 1f);
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
            Instantiate(particle, ParticleParent.transform);
            
            HandleChange();
        }

        public void OnEventStart(object sender, WorldEventProperties properties)
        {
            var @event = sender as WorldEvent;
            if (@event == null || EventOngoing)
            {
                print("Event is null or ongoing.");
                return;
            }

            EventPrefab selectedEvent = @event.EventPrefab;
            EventOngoing = true;
            SetWeather(selectedEvent.Prefab.GetComponent<RainDataHolder>().WeatherData);
        }        
    }
}

