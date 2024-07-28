using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace UZSG.World.Weather
{
    [Serializable]
    public struct WeatherAttributes
    {
        public string Name;
        public int Strength;
        public int DurationSeconds;
    }

    [Serializable]
    public struct WeatherProperties
    {
        public Color DayFogColor;
        public Color NightFogColor;
        public float Temperature;
    }


    [CreateAssetMenu(fileName = "New Weather Data", menuName = "UZSG/Weather Data")]
    [Serializable]
    public class WeatherData : ScriptableObject
    {
        public ParticleSystem particles;
        public WeatherAttributes weatherAttributes;
        public WeatherProperties weatherProperties;
    }

}

