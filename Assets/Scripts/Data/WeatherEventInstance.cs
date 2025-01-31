using System;
using System.Collections.Generic;

using UnityEngine;
using UZSG.Worlds;
using UZSG.Worlds.Events;

namespace UZSG.Data
{
    [Serializable]
    public struct WeatherEventInstance
    {
        public string Name;
        public WeatherData WeatherData;
        public WeatherData WeatherPreset;
        public float ChanceToOccur;
    }
}