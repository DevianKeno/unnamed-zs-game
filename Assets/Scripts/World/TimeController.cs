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
        public float CurrentTime;
        public int CurrentDay;
        public int DayLength = 2160;

        public void Initialize()
        {
            Game.Tick.OnTick += OnTick;
            CurrentTime = 0;
            CurrentDay = 0;
        }

        private void OnTick(TickInfo info)
        {
            float tickThreshold = Game.Tick.TPS / 64f;

            CurrentTime += ((Game.Tick.SecondsPerTick * (Game.Tick.CurrentTick / 32f)) * tickThreshold);
            CalculateDay();
            UpdateCelestialBodies();
        }

        public void CalculateDay()
        {
            if (CurrentTime > DayLength)
            {
                CurrentTime = 0;
                CurrentDay++;
            }
        }

        private void OnValidate()
        {
            if (CurrentTime > DayLength)
            {
                CurrentTime = 0;
                CurrentDay++;
            }

            UpdateCelestialBodies();
        }

        public void UpdateCelestialBodies()
        {
            float timeOfDay = CurrentTime / DayLength;
            float timeOfNight = CurrentTime / DayLength;
            float sunAngle = timeOfDay * 360f;
            float moonAngle = (timeOfDay + 0.5f) % 1f * 360f;

            // Update Sun
            Vector3 sunRotation = new Vector3(sunAngle, 0, 0);
            SunLight.transform.rotation = Quaternion.Euler(sunRotation);

            // Update Moon
            Vector3 moonRotation = new Vector3(moonAngle, 0, 0);
            MoonLight.transform.rotation = Quaternion.Euler(moonRotation);

            /// Sample the color from the gradient for the Sun
            Color sunColor = DayColors.Evaluate(timeOfDay);
            SunLight.color = sunColor;

            Color moonColor = NightColors.Evaluate(timeOfNight);
            MoonLight.color = moonColor;

            /// Sun emits light between 0 and 180 degrees
            if (sunAngle >= 0 && sunAngle <= 180)
            {
                SunLight.intensity = 4;
            }
            else
            {
                SunLight.intensity = 0;
            }

            /// Moon emits light between 180 and 360 degrees
            if (moonAngle >= 0 && moonAngle <= 180)
            {
                MoonLight.intensity = 1;
            }
            else
            {
                MoonLight.intensity = 0;
            }
        }
    }
}