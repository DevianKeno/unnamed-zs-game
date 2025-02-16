using System;

using UnityEngine;
using UnityEngine.Rendering;

namespace UZSG.Data
{
    /// <summary>
    /// Represents a particular type of weather.
    /// Values are set in Inspector; <b>Do not write</b>.
    /// </summary>
    [Serializable]
    [CreateAssetMenu(fileName = "New Weather Data", menuName = "UZSG/Weather Datra")]
    public class WeatherData : BaseData
    {
        public string DisplayName;

        [Header("Skybox & Lighting")]
        public Material Skybox;
        public AmbientMode AmbientMode;

        [Header("Trilight Color Settings")]
        public Gradient SkyColorDay;
        public Gradient SkyColorNight;
        public Gradient EquatorColorDay;
        public Gradient EquatorColorNight;
        public Gradient GroundColorsDay;
        public Gradient GroundColorsNight;
        
        [Space, Header("Colors")]
        public Gradient DayColors;
        public Gradient DayFogColor;
        public AnimationCurve DayFogDensity;
        public Gradient DaySkyboxTint;
        public Gradient NightColors;
        public Gradient NightFogColor;
        public AnimationCurve NightFogDensity;
        public Gradient NightSkyboxTint;

        [Space]
        public GameObject WeatherParticlesPrefab;
    }
}