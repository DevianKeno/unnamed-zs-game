using System;

using UnityEngine;
using UnityEngine.Serialization;


using UZSG.Saves;
using UZSG.Data;
using UZSG.UI;

namespace UZSG.Worlds
{
    public class TimeController : MonoBehaviour
    {
        public const int MORNING_HOUR = 6;
        public const int NOON_HOUR = 12;
        public const int EVENING_HOUR = 18;
        public const int NIGHT_HOUR = 21;
        public const int MIDNIGHT_HOUR = 0;

        /// Sun is at a specific angle at specific times of the day
        public class SunAngles
        {
            public const int Morning = 5;
            public const int Noon = 90;
            public const int Evening = 170;
            public const int Night = 205;
            public const int Midnight = 270;
        }

        [Header("Celestial Bodies")]
        [SerializeField, FormerlySerializedAs("SunLight")] Light sunLight;
        public float SunlightIntensity = 3f;
        [SerializeField, FormerlySerializedAs("MoonLight")] Light moonLight;
        public float MoonlightIntensity = 1f;


        #region Time properties

        public bool IsDay => currentHour >= WorldAttributes.DAY_START_HOUR && currentHour < WorldAttributes.NIGHT_START_HOUR;
        public bool IsNight => currentHour >= WorldAttributes.NIGHT_START_HOUR || currentHour < WorldAttributes.DAY_START_HOUR;
        public bool IsMorning => currentHour >= MORNING_HOUR && currentHour < NOON_HOUR;
        public bool IsAfternoon => currentHour >= NOON_HOUR && currentHour < EVENING_HOUR;
        public bool IsEvening => currentHour >= EVENING_HOUR && currentHour < NIGHT_HOUR;
        /// <summary>
        /// Length of day time in raw time.
        /// </summary>
        int _dayLength = WorldAttributes.DEFAULT_DAY_LENGTH;
        /// <summary>
        /// Length of night time in raw time.
        /// </summary>
        int _nightLength = WorldAttributes.DEFAULT_NIGHT_LENGTH;
        /// <summary>
        /// Length of Day in raw time.
        /// </summary>
        int totalDayLength => _dayLength + _nightLength;
        /// <summary>
        /// The raw time from 0 it takes to reach "Day time" (6:00).
        /// </summary>
        int dayStartRawTime => WorldAttributes.DAY_START_HOUR * rawTimePerHour;
        /// <summary>
        /// The raw time from 0 it takes to reach "Night time" (21:00).
        /// </summary>
        int nightStartRawTime => WorldAttributes.NIGHT_START_HOUR * rawTimePerHour;
        int rawTimePerHour => totalDayLength / 24;
        int rawTimePerMinute => rawTimePerHour / 60;
        int rawTimePerSecond => rawTimePerMinute / 60;
        /// <summary>
        /// Raw time elapsed from 6:00 (by default) onwards.
        /// </summary>
        float rawTimeSinceSunrise => RawTime - dayStartRawTime;
        /// <summary>
        /// Raw time elapsed from 21:00 (by default) onwards.
        /// </summary>
        float rawTimeSinceNightfall => RawTime - nightStartRawTime;

        #endregion


        [Header("Time")]
        [SerializeField] bool zeroTimeStartsAtDay = true;
        [SerializeField] float rawTime = 0;
        public float RawTime
        {
            get => zeroTimeStartsAtDay ? rawTime + dayStartRawTime : rawTime;
            set => rawTime = value;
        }
        [SerializeField] int currentHour;
        public int Hour
        {
            get => currentHour;
            set => currentHour = Math.Clamp(value, 0, 23);
        }
        [SerializeField] int currentMinute;
        public int Minute
        {
            get => currentMinute;
            set => currentMinute = Math.Clamp(value, 0, 59);
        }
        [SerializeField] int currentSecond;
        public int Second
        {
            get => currentSecond;
            set => currentSecond = Math.Clamp(value, 0, 59);
        }
        [SerializeField] int currentDay = 0;
        public int CurrentDay
        {
            get => currentDay;
            set => currentDay = value;
        }
        int _lastHour;
        int _lastMinute;


        [Header("Colors")]
        [SerializeField] internal WeatherData WeatherPreset;


        #region Events
        /// <summary>
        /// Called everytime a new day passes.
        /// <c>int</c> is the current day.
        /// </summary>
        public event Action<int> OnDayPassed;
        /// <summary>
        /// Called everytime a new hour passes.
        /// <c>int</c> is the current hour.
        /// </summary>
        public event Action<int> OnHourPassed;
        /// <summary>
        /// Called everytime a new minute passes.
        /// <c>int</c> is the current minute.
        /// </summary>
        public event Action<int> OnMinutePassed;
        /// <summary>
        /// Called everytime a new second passes.
        /// <c>int</c> is the current second.
        /// </summary>
        public event Action<int> OnSecondPassed;

        #endregion


        #region Initializing methods

        public void InitializeFromSave(WorldSaveData saveData)
        {
            if (saveData == null) return;
            
            // this._dayLength = Math.Clamp(saveData.DayLengthSeconds, WorldAttributes.MIN_DAY_LENGTH, WorldAttributes.MAX_DAY_LENGTH);
            // this._nightLength = Math.Clamp(saveData.NightLengthSeconds, WorldAttributes.MIN_NIGHT_LENGTH, WorldAttributes.MAX_NIGHT_LENGTH);
            this.currentDay = Math.Clamp(saveData.Day, 0, int.MaxValue); /// how about eons
            SetTime(saveData.Hour, saveData.Minute, saveData.Second);
        }

        internal void Initialize()
        {
            
        }

        internal void Deinitialize()
        {
            
        }

        #endregion


        void OnValidate()
        {
            if (gameObject.activeInHierarchy)
            {
                ValidateFields();
                SetTimeRaw(rawTime);
                UpdateCelestialBodies();
                UpdateFog();
            }
        }

        void ValidateFields()
        {
            //
        }

        void UpdateTime()
        {
            currentHour = Mathf.FloorToInt(RawTime / rawTimePerHour) % 24;
            float hourProgress = (RawTime % rawTimePerHour) / rawTimePerHour;
            currentMinute = Mathf.FloorToInt(hourProgress * 60); /// Minutes within the current hour
            float minuteProgress = (hourProgress * 60) % 1;
            currentSecond =  Mathf.FloorToInt(minuteProgress * 60);

            if (rawTime >= totalDayLength)
            {
                rawTime = 0;
                CurrentDay++;
                OnDayPassed?.Invoke(CurrentDay);
            }
            else if (rawTime < 0)
            {
                rawTime = totalDayLength - 1;
                CurrentDay--;
            }
            
            if (currentHour != _lastHour)
            {
                _lastHour = currentHour;
                OnHourPassed?.Invoke(currentHour);
            }

            if (currentMinute != _lastMinute)
            {
                _lastMinute = currentMinute;
                OnMinutePassed?.Invoke(currentMinute);
            }
        }

        internal void Tick(float deltaTime)
        {
            rawTime += deltaTime;
            
            UpdateTime();
            UpdateCelestialBodies();
            UpdateFog();
        }

        void UpdateCelestialBodies()
        {
            float sunAngle = 0f;

            /// TODO: to lerp
            sunLight.intensity = IsDay ? SunlightIntensity : 0;

            /// Sun is at a specific angle at specific times of the day
            if (IsMorning) /// 6:00 to 12:00
            {
                sunAngle = CalculateSunAngle(
                    dayStartRawTime, /// Raw time from 0 it takes to reach 6:00 
                    NOON_HOUR * rawTimePerHour, /// Raw time from 0 it takes to reach 12:00
                    SunAngles.Morning, SunAngles.Noon); /// Sun angles to lerp
            }
            else if (IsAfternoon) /// 12:00 to 18:00
            {
                sunAngle = CalculateSunAngle(
                    NOON_HOUR * rawTimePerHour, 
                    EVENING_HOUR * rawTimePerHour, /// Raw time from 0 it takes to reach 18:00 
                    SunAngles.Noon, SunAngles.Evening);
            }
            else if (IsEvening) /// 18:00 to 21:00
            {
                sunAngle = CalculateSunAngle(
                    EVENING_HOUR * rawTimePerHour,
                    NIGHT_HOUR * rawTimePerHour, /// Raw time from 0 it takes to reach 21:00 
                    SunAngles.Evening, SunAngles.Night);
            
                /// Here's the point the Sun light's intensity
                /// lerps to 0 towards nightfall
                if (sunAngle > SunAngles.Evening)
                {
                    var t = Mathf.InverseLerp(SunAngles.Evening, 180f, sunAngle);
                    sunLight.intensity = Mathf.Lerp(SunlightIntensity, 0, t);
                }
            }
            else /// Nighttime to early morning, 21:00 to 6:00
            {
                sunAngle = CalculateSunAngle(
                    NIGHT_HOUR * rawTimePerHour,
                    totalDayLength + dayStartRawTime, /// Raw time from 0 it takes to reach 6:00 
                    SunAngles.Night, 360f + SunAngles.Morning);
                    
                /// Here's another point the Sun light's intensity
                /// lerps to SunLightIntensity towards sunrise
                if ((sunAngle % 360f) >= 0f && (sunAngle % 360f) < SunAngles.Morning)
                {
                    var t = Mathf.InverseLerp(0f, SunAngles.Morning, sunAngle % 360f);
                    sunLight.intensity = Mathf.Lerp(0, SunlightIntensity, t);
                }
            }

            sunLight.transform.eulerAngles = new Vector3(sunAngle % 360f, 0, 0);
            UpdateSunLight();
            UpdateMoonLight();
        }

        float CalculateSunAngle(float startRawTime, float endRawTime, float startAngle, float endAngle)
        {
            float timeElapsed = RawTime - startRawTime;
            float totalTime = endRawTime - startRawTime;

            return Mathf.Lerp(startAngle, endAngle, timeElapsed / totalTime);
        }

        /// <summary>
        /// Currently the Sun Light's intensity is calculated with the angle (in the UpdateCelestialBodies() method).
        /// As opposed to the Moon Light's method (UpdateMoonLight()) which includes both color and intensity calculations.
        /// </summary>
        void UpdateSunLight()
        {
            float t;
            if (IsDay)
            {
                t = rawTimeSinceSunrise / _dayLength;
            }
            else
            {
                t = rawTimeSinceNightfall / _nightLength;
            }
            sunLight.color = WeatherPreset.DayColors.Evaluate(t);;
        }

        void UpdateMoonLight()
        {
            Color targetColor;
            float targetIntensity = MoonlightIntensity;

            if (IsEvening)
            {
                /// Moon light's intensity lerps from Evening towards Nightfall (0 -> MoonLightIntensity)
                var timeSinceEvening = RawTime - (EVENING_HOUR * rawTimePerHour);
                var timeFromEveningToNight = nightStartRawTime - (EVENING_HOUR * rawTimePerHour);
                targetIntensity = Mathf.Lerp(0f, MoonlightIntensity, timeSinceEvening / timeFromEveningToNight);
                targetColor = WeatherPreset.NightColors.Evaluate(0f);
            }
            else if (IsNight)
            {
                /// Moon light's intensity lerps from Midnight* towards Sunrise (MoonLightIntensity -> 0)
                var timeSinceMidnight = RawTime - (24f * rawTimePerHour);
                var timeFromMidnightToSunrise = totalDayLength - nightStartRawTime;
                targetIntensity = Mathf.Lerp(MoonlightIntensity, 0f, timeSinceMidnight / timeFromMidnightToSunrise);
                targetColor = WeatherPreset.NightColors.Evaluate(rawTimeSinceNightfall / _nightLength);
            }
            else
            {
                targetColor = WeatherPreset.NightColors.Evaluate(0f);
            }
            
            moonLight.color = targetColor;
            moonLight.intensity = targetIntensity;
        }
        
        void UpdateFog()
        {
            Color fogColor;
            float fogDensity;
            if (IsDay)
            {
                var t = rawTimeSinceSunrise / _dayLength;
                fogColor = WeatherPreset.DayFogColor.Evaluate(t);
                fogDensity = WeatherPreset.DayFogDensity.Evaluate(t);
            }
            else if (IsNight)
            {
                var t = rawTimeSinceNightfall / _nightLength;
                fogColor = WeatherPreset.NightFogColor.Evaluate(t);
                fogDensity = WeatherPreset.NightFogDensity.Evaluate(t);
            }
            else
            {
                fogColor = Colors.Transparent;
                fogDensity = 0;
            }
            RenderSettings.fogColor = fogColor;
            RenderSettings.fogDensity = fogDensity;
        }

        #region Public methods

        public WorldTime GetWorldTime()
        {
            return new()
            {
                Day = this.currentDay,
                Hour = this.currentHour,
                Minute = this.currentMinute,
                Second = this.currentSecond,
            };
        }

        public void SetTimeRaw(float value)
        {
            RawTime = value;
            UpdateTime();
            UpdateCelestialBodies();
        }

        public void SetTime(int hour, int minute)
        {
            Hour = hour % 24;
            Minute = minute % 60;
            rawTime = (hour * rawTimePerHour) + (minute * rawTimePerMinute);
            if (zeroTimeStartsAtDay) rawTime -= dayStartRawTime;

            UpdateCelestialBodies();
        }

        public void SetTime(int hour, int minute, int second)
        {
            Hour = hour % 24;
            Minute = minute % 60;
            Second = second % 60;
            rawTime = (Hour * rawTimePerHour) + (Minute * rawTimePerMinute) + (Second * rawTimePerSecond);
            if (zeroTimeStartsAtDay) rawTime -= dayStartRawTime;

            UpdateCelestialBodies();
        }

        public void ShowTime()
        {
            Debug.Log("Current Time: " + RawTime);
        }
        
        #endregion
    }
}