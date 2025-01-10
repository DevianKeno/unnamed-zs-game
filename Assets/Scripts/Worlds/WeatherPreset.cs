using System;
using System.Collections.Generic;

using UnityEditor;
using UnityEngine;

namespace UZSG.Worlds
{
    /// <summary>
    ///
    /// </summary>
    [Serializable]
    [CreateAssetMenu(fileName = "New Weather Preset", menuName = "UZSG/Weather Preset")]
    public class WeatherPreset : ScriptableObject
    {
        public Gradient DayColors;
        public Gradient DayFogColor;
        public AnimationCurve DayFogDensity;
        public Gradient NightColors;
        public Gradient NightFogColor;
        public AnimationCurve NightFogDensity;
        public Material Skybox;
    }
}