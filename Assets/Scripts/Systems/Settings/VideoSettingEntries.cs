using System;

using UnityEngine;

namespace UZSG.Settings
{
    #region Video Settings

    public class MonitorSetting : SettingEntry
    {
        public int Monitor;

        public override void Load()
        {
            Monitor = PlayerPrefs.GetInt("monitor", 0);
            Apply(Monitor);
        }

        public override void Save()
        {
            PlayerPrefs.SetInt("monitor", Monitor);
        }
        
        public override void Apply(object value)
        {
            if (value is int valueInt)
            {
                if (Display.displays.IsValidIndex(valueInt))
                {
                    Display.displays[valueInt].Activate();
                }
            }
        }
    }

    public class WindowModeSetting : SettingEntry
    {
        public FullScreenMode Mode;

        public override void Load()
        {
            var modeInt = GetModeFromInt(PlayerPrefs.GetInt("fullscreen_mode", 0));
            Mode = modeInt;
            Apply(modeInt);
        }

        public override void Save()
        {
            PlayerPrefs.SetInt("fullscreen_mode", GetIntFromMode(Mode));
        }

        public override void Apply(object value)
        {
            if (value is int valueInt)
            {
                Screen.fullScreenMode = GetModeFromInt(valueInt);
            }
        }

        int GetIntFromMode(FullScreenMode mode)
        {
            return mode switch
            {
                FullScreenMode.ExclusiveFullScreen => 0,
                FullScreenMode.Windowed => 1,
                FullScreenMode.FullScreenWindow => 2,
                _ => 0
            };
        }

        FullScreenMode GetModeFromInt(int value)
        {
            return value switch
            {
                0 => FullScreenMode.ExclusiveFullScreen,
                1 => FullScreenMode.Windowed,
                2 => FullScreenMode.FullScreenWindow,
                _ => FullScreenMode.ExclusiveFullScreen
            };
        }
    }

    public class VSyncSetting : SettingEntry
    {
        public bool Enable;

        public override void Load()
        {
            Enable = PlayerPrefs.GetInt("v_sync", 1) > 0;
            Apply(Enable);
        }

        public override void Save()
        {
            PlayerPrefs.SetInt("v_sync", Enable ? 1 : 0);
        }
        
        public override void Apply(object value)
        {
            if (value is bool valueBool)
            {
                QualitySettings.vSyncCount = valueBool ? 1 : 0;
            }
        }
    }

    public class FramerateCapSetting : SettingEntry
    {
        public const double MIN_FPS = 30;
        public const double MAXMAX_FPS = 999; /// lolinsane rfr
        
        public double Value;

        public override void Load()
        {
            Value = PlayerPrefs.GetInt("framerate_cap", Mathf.RoundToInt((float) Screen.currentResolution.refreshRateRatio.value));
            Apply((float) Value);
        }

        public override void Save()
        {
            PlayerPrefs.SetInt("framerate_cap", Application.targetFrameRate);
        }

        public override void Apply(object value)
        {
            if (value is not float valueFloat) return;

            var maxRR = (float) Screen.currentResolution.refreshRateRatio.value;

            if (valueFloat > maxRR)
            {
                Application.targetFrameRate = -1; /// No cyap
            }
            else
            {
                var fps = Math.Clamp(valueFloat, MIN_FPS, maxRR);
                Application.targetFrameRate = (int) Math.Floor(fps);
            }
        }
    }

    public class ResolutionSetting : SettingEntry
    {
        
    }

    #endregion
}