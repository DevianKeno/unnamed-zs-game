using System;
using System.Collections.Generic;
using UnityEngine;
using UZSG.Data;

namespace UZSG.WorldEvents
{    
    [Serializable]
    public class EventPrefab<T>
    {
        List<T> selectedEvents = new();
    }

    [Serializable]
    public struct WeatherInstanceProperties
    {
        public WeatherData WeatherData;
        public float ChanceToOccur;
    }

    [Serializable]
    public struct RaidInstanceProperties
    {
        public EnemyData EnemyData;
        public float ChanceToOccur;
    }
}