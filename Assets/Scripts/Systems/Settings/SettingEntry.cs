using System;
using UnityEngine;
using UZSG.Systems;

namespace UZSG
{
    public abstract class SettingEntry
    {
        public virtual void Apply() { }
        public virtual void Load() { }
        public virtual void Save() { }
    }
    
    public enum SettingQualityValue {
        VeryLow, Low, Medium, High, VeryHigh, Ultra
    }

    public class WindowModeSetting : SettingEntry
    {
        public int Value;

        public override void Apply()
        {
            SetWindowMode(Value);
        }

        void SetWindowMode(int index)
        {
            var mode = index switch
            {
                0 => FullScreenMode.ExclusiveFullScreen, // Exclusive Fullscreen
                1 => FullScreenMode.Windowed,           // Windowed
                2 => FullScreenMode.FullScreenWindow,   // Borderless Windowed
                _ => FullScreenMode.ExclusiveFullScreen
            };

            Screen.fullScreenMode = mode;
            PlayerPrefs.SetInt("WindowMode", index);
            PlayerPrefs.Save();

            Debug.Log($"Window Mode Set: {mode}");
        }
    }

    public class VSyncSetting : SettingEntry
    {
        public bool Enable;
        
        public override void Apply()
        {
            QualitySettings.vSyncCount = Enable ? 1 : 0;
        }
    }

    public class FramerateCapSetting : SettingEntry
    {
        public const double MIN_FPS = 30;
        public const double MAXMAX_FPS = 360; /// lol?
        
        public double Value;

        public override void Apply()
        {
            double maxRR = Screen.currentResolution.refreshRateRatio.value;

            if (Value > maxRR)
            {
                Application.targetFrameRate = -1; /// No cyap
            }
            else
            {
                Value = Math.Clamp(Value, MIN_FPS, maxRR);
                Application.targetFrameRate = (int) Math.Floor(Value);
            }
        }

        public override void Save()
        {
            PlayerPrefs.SetInt("framerate_cap", 1);
        }
    }

    public class MonitorSetting : SettingEntry
    {
        public int SelectedResolutionIndex;
        
        public override void Apply()
        {
            
        }

        public void SetResolution(int index)
        {
            // if (index < 0 || index >= availableResolutions.Length) return;

            // Resolution selectedResolution = availableResolutions[index];
            // Screen.SetResolution(
            //     selectedResolution.width,
            //     selectedResolution.height,
            //     FullScreenMode.ExclusiveFullScreen,
            //     selectedResolution.refreshRateRatio);
        }

        public override void Save()
        {
            PlayerPrefs.SetInt("resolution", SelectedResolutionIndex);
        }

        public void SetFullscreen(bool isFullscreen)
        {
            Screen.fullScreen = isFullscreen;
            PlayerPrefs.SetInt("Fullscreen", isFullscreen ? 1 : 0);
            PlayerPrefs.Save();

            Debug.Log($"Fullscreen mode: {(isFullscreen ? "Enabled" : "Disabled")}");
        }
    }

    public class ResolutionSetting : SettingEntry
    {
        
    }

    public class ShadowResolutionSetting : SettingEntry
    {
        public ShadowResolution Resolution;
        
        public override void Apply()
        {
            QualitySettings.shadowResolution = Resolution;
        }
    }

    #region Accesisbility Settings

    public class LanguageSetting : SettingEntry
    {
        public int Value;
        
        public override void Apply()
        {
            Game.Locale.SetLocalization("");
        }
    }

    #endregion
}