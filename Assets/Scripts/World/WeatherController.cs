using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UZSG.Systems;
using UZSG.WorldBuilder;

namespace UZSG.World.Weather
{
    public class WeatherController : MonoBehaviour
    {

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
            CurrentWeather = CurrentWeather == null ? DefaultWeather : CurrentWeather;
            _currentParticleSystem = CurrentWeather.particleSystem;
            SetWeather(CurrentWeather);
            Game.Tick.OnTick += OnTick;
            
        }
        void OnValidate()
        {
            CurrentWeather = CurrentWeather == null ? DefaultWeather : CurrentWeather;
            _currentParticleSystem = CurrentWeather.particleSystem;
            SetWeather(CurrentWeather);
        }
        void OnTick(TickInfo info)
        {
            float tickThreshold = Game.Tick.TPS / 64f;
            if (_weatherCountdown > 0 || _weatherCountdown != -1)
            {
                _weatherCountdown -= ((Game.Tick.SecondsPerTick * (Game.Tick.CurrentTick / 32f)) * tickThreshold);
            }

            HandleChange();
        }

        void HandleChange()
        {
            if (_weatherCountdown <= 0f || _weatherCountdown == -1f)
            {
                if (_currentWeather != _defaultWeather)
                {
                    SetWeather(_defaultWeather);
                }
            }

            Time.DayFogColor = Color.Lerp(Time.DayFogColor, _currentWeather.weatherProperties.DayFogColor, 1f);
            Time.NightFogColor = Color.Lerp(Time.NightFogColor, _currentWeather.weatherProperties.NightFogColor, 1f);
        }

        public void SetWeather(WeatherData weather)
        {
            _currentWeather = weather;
            CurrentWeather = weather;
            _weatherDuration = _currentWeather.weatherAttributes.DurationSeconds;
            _weatherCountdown = _weatherDuration;
            _currentParticleSystem = weather.particleSystem;
            
            if (ParticleParent.transform.childCount > 0) Destroy(ParticleParent.transform.GetChild(0).gameObject);

            ParticleSystem particle = _currentParticleSystem;
            Instantiate(particle, ParticleParent.transform);
            
            HandleChange();

        }
    }
}

