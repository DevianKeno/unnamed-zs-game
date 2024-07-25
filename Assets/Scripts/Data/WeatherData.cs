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
        public int Duration;
        public float Temperature;
    }

    [Serializable]
    public struct WeatherProperties
    {
        public bool isWahahaha;
    }

    [CreateAssetMenu(fileName = "New Weather Data", menuName = "UZSG/Weather Data")]
    [Serializable]
    public class WeatherData : ScriptableObject
    {
        public List<WeatherAttributes> weatherAttributes;
        public WeatherProperties weatherProperties;
    }

}

