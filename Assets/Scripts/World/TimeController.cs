using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UZSG.Systems;

namespace UZSG.WorldBuilder
{
    public class TimeController : MonoBehaviour
    {
        public Light SunLight;
        public Light MoonLight;
        public Gradient DayColors;
        public Gradient NightColors;
        public Color DayFogColor;
        public Color NightFogColor;
        public int TwentyFourHourTime;
        [SerializeField] float _currentTime;
        public float CurrentTime
        {
            get { return _currentTime; } 
        }

        public int CurrentDay;
        int _dayLength = 2160;

        
        public void Initialize()
        {
            Game.Tick.OnTick += OnTick;
            _currentTime = 0;
            CurrentDay = 0;
        }

        void OnValidate()
        {
            if (Application.isPlaying) return;
            if (_currentTime > _dayLength)
            {
                TwentyFourHourTime = 0;
                CurrentDay++;
            }

            UpdateCelestialBodies();

            SetTime(TwentyFourHourTime);
        }

        void OnTick(TickInfo info)
        {
            float tickThreshold = Game.Tick.TPS / 64f;

            _currentTime += ((Game.Tick.SecondsPerTick * (Game.Tick.CurrentTick / 32f)) * tickThreshold);
            TwentyFourHourTime = Mathf.FloorToInt(_currentTime / _dayLength * 2400f);
            IncrementDay();
            UpdateCelestialBodies();
        }

        void IncrementDay()
        {
            if (_currentTime > _dayLength)
            {
                _currentTime = 0;
                CurrentDay++;
            }
        }

        void UpdateCelestialBodies()
        {
            float time = _currentTime / _dayLength;
            float sunAngle = (time * 360f + 270f) % 360f;
            float moonAngle = (sunAngle + 180f) % 360f;

            Vector3 sunRotation = new Vector3(sunAngle, 0, 0);
            SunLight.transform.rotation = Quaternion.Euler(sunRotation);

            Vector3 moonRotation = new Vector3(moonAngle, 0, 0);
            MoonLight.transform.rotation = Quaternion.Euler(moonRotation);

            UpdateSunLight(sunAngle, time);
            UpdateMoonLight(moonAngle, time);
            UpdateFogColor(time);
        }

        void UpdateSunLight(float sunAngle, float timeOfDay)
        {
            Color sunColor = DayColors.Evaluate(timeOfDay);
            SunLight.color = sunColor;

            if (sunAngle >= 0 && sunAngle <= 180)
            {
                SunLight.intensity = 4;
            }
            else
            {
                SunLight.intensity = 0;
            }
        }
        void UpdateMoonLight(float moonAngle, float timeOfNight)
        {
            Color moonColor = NightColors.Evaluate(timeOfNight);
            MoonLight.color = moonColor;

            if (moonAngle >= 0 && moonAngle <= 180)
            {
                MoonLight.intensity = 1;
            }
            else
            {
                MoonLight.intensity = 0;
            }
        }

        void UpdateFogColor(float time)
        {
            const float dayStart = 0.25f;
            const float dayEnd = 0.75f;

            Color fogColor;
            float fogDensity;

            if (time >= dayStart && time < dayEnd)
            {
                fogColor = DayFogColor;
                fogDensity = 0.005f;
            }
            else
            {
                fogColor = NightFogColor;
                fogDensity = 0.05f;
            }

            if (time < dayStart)
            {
                float t = Mathf.InverseLerp(0f, dayStart, time);
                fogColor = Color.Lerp(NightFogColor, DayFogColor, t);
                fogDensity = Mathf.Lerp(0.05f, 0.005f, t);
            }
            else if (time >= dayEnd)
            {
                float t = Mathf.InverseLerp(dayEnd, 1f, time);
                fogColor = Color.Lerp(DayFogColor, NightFogColor, t);
                fogDensity = Mathf.Lerp(0.005f, 0.05f, t);
            }

            RenderSettings.fogColor = fogColor;
            RenderSettings.fogDensity = fogDensity;
        }

        public void SetTime(int time)
        {
            float dayFraction = (float)time / 2400f;
            _currentTime = dayFraction * _dayLength;

            UpdateCelestialBodies();
        }

        public void ShowTime()
        {
            Debug.Log("Current Time: " + TwentyFourHourTime);
        }
    }
}