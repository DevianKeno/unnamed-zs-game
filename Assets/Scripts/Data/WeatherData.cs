using System;

using UnityEngine;
using UZSG.Data;

namespace UZSG.Data
{
    /// <summary>
    /// Represents a particular type of weather.
    /// </summary>
    [Serializable]
    [CreateAssetMenu(fileName = "New Weather Data", menuName = "UZSG/Weather Datra")]
    public class WeatherData : BaseData
    {
        public string DisplayName;

        [Space, Header("Colors")]
        public Gradient DayColors;
        public Gradient DayFogColor;
        public AnimationCurve DayFogDensity;
        public Gradient NightColors;
        public Gradient NightFogColor;
        public AnimationCurve NightFogDensity;

        [Space]
        public GameObject WeatherParticlesPrefab;
        public Material Skybox;
    }
}