using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using TMPro;

using UZSG.UI;
using UZSG.Data;

namespace UZSG
{
    public class SettingsManager : MonoBehaviour
    {
        bool _isInitialized = false;
        GlobalSettingsWindow userInterface;
        [SerializeField] GameObject globalSettingsWindow;

        Dictionary<SettingCategory, List<SettingsEntryData>> settingsDataDict = new();

        internal void Initialize()
        {
            if (_isInitialized) return;

            _isInitialized = true;
            userInterface = Instantiate(globalSettingsWindow, parent: Game.UI.Canvas.transform).GetComponent<GlobalSettingsWindow>();
            userInterface.name = "Global Settings Window";
            userInterface.Initialize();
            userInterface.Hide();

            InitializeSettings();

            foreach (var setting in Resources.LoadAll<SettingsEntryData>("Data/Settings"))
            {
                if (!settingsDataDict.TryGetValue(setting.Category, out var list))
                {
                    list = new();
                }
                
                list.Add(setting);
            }

            LoadGlobalSettings();
        }

        void InitializeSettings()
        {
            InitializeVideoSettings();
            InitializeAccessibilitySettings();
        }

        void InitializeAccessibilitySettings()
        {
            GetLanguageOptions();
        }

        void InitializeVideoSettings()
        {
            GetMonitorOptions();
            GetWindowModeOptions();
            GetResolutionOptions();
            GetVSyncOptions();
            GetFramerateCapOptions();
        }


        #region Public methods

        public void ShowGlobalInterface()
        {
            userInterface.Show();
        }

        public void HideGlobalInterface()
        {
            userInterface.Hide();
        }

        public void LoadGlobalSettings()
        {
            LoadVideoSettings();
            LoadAccessibilitySettings();
        }

        public void SaveGlobalSettings()
        {
            SaveAudioSettings();
            SaveVideoSettings();
            SaveGraphicsSettings();
            SaveControlsSettings();
            SaveAccessibilitySettings();

            PlayerPrefs.Save();
        }

        #endregion


        void LoadVideoSettings()
        {
            QualitySettings.shadowResolution = (ShadowResolution) PlayerPrefs.GetInt("shadows_quality", (int) ShadowResolution.Medium);
        }

        void LoadAccessibilitySettings()
        {
            Game.Locale.SetLocalization(PlayerPrefs.GetString("language", "en_us"));
        }

        void SaveVideoSettings()
        {
            SettingEntryUI ui;

            foreach (var setting in settingsDataDict[SettingCategory.Accessibility])
            {
                if (TryGetEntryUI(setting.Id, out ui) && ui._isDirty)
                {
                    if (ui.Setting == null)
                    {
                        Debug.LogWarning($"SettingEntry for '{setting.Id}' is null!");
                        return;
                    }

                    ui.Setting.Apply();
                    ui.Setting.Save();
                }
            }
        }

        public enum ShadowDistance {
            Low = 16,
            Medium = 32,
            High = 64,
            VeryHigh = 128,
            Ultra = 256,
        }

        void SaveGraphicsSettings()
        {
        }

        void SaveControlsSettings()
        {
        }

        void SaveAudioSettings()
        {
        }

        void SaveAccessibilitySettings()
        {
        }


        #region Audio  Settings



        #endregion

    
        #region Video Settings

        void GetMonitorOptions()
        {
            if (!TryGetEntryUI("monitor", out var ui)) return;
            
            ui.Setting = new MonitorSetting();
            var dropdown = (ui as SettingEntryDropdownUI).Dropdown;
            var options = new List<TMP_Dropdown.OptionData>();

            int i = 0;
            foreach (var display in Display.displays)
            {
                options.Add(new(text: $"Monitor {i}: "));
                i++;
            }
            
            dropdown.ClearOptions();
            dropdown.AddOptions(options);
        }
        
        void GetWindowModeOptions()
        {
            if (!TryGetEntryUI("fullscreen_mode", out var ui)) return;
            
            ui.Setting = new MonitorSetting();
            var dropdown = (ui as SettingEntryDropdownUI).Dropdown;

            dropdown.ClearOptions();
            dropdown.AddOptions(new List<string>()
            {
                "Exclusive Fullscreen",
                "Windowed",
                "Borderless Windowed"
            });
        }

        void GetResolutionOptions()
        {
            if (!TryGetEntryUI("resolution", out var ui)) return;

            var availableResolutions = Screen.resolutions.Distinct().ToArray();
            var options = new List<string>();

            ui.Setting = new ResolutionSetting();
            var dropdown = (ui as SettingEntryDropdownUI).Dropdown;

            int selected = 0;
            for (int i = 0; i < availableResolutions.Length; i++)
            {
                string resolutionText = $"{availableResolutions[i].width} x {availableResolutions[i].height} @{availableResolutions[i].refreshRateRatio} Hz";
                options.Add(resolutionText);

                if (availableResolutions[i].width == Screen.currentResolution.width &&
                    availableResolutions[i].height == Screen.currentResolution.height)
                {
                    selected = i;
                }
            }

            dropdown.ClearOptions();
            dropdown.AddOptions(options);
            dropdown.value = selected;
        }

        void GetVSyncOptions()
        {
            if (!TryGetEntryUI("v_sync", out var ui)) return;

            ui.Setting = new VSyncSetting(); 
            var toggle = (ui as SettingEntryToggleUI).Toggle;
            toggle.isOn = QualitySettings.vSyncCount > 0;
        }

        void GetFramerateCapOptions()
        {
            if (!TryGetEntryUI("framerate_cap", out var ui)) return;

            ui.Setting = new FramerateCapSetting(); 
            var slider = (ui as SettingEntrySliderUI).Slider;
            slider.minValue = Mathf.FloorToInt((float) FramerateCapSetting.MIN_FPS);
            slider.maxValue = Mathf.FloorToInt((float) Screen.currentResolution.refreshRateRatio.value) + 1; /// +1 for no cyap 
            slider.value = Application.targetFrameRate < 0 ? slider.maxValue : slider.value;
        }

        #endregion


        #region Graphics Settings

        void GetAmbientOcclusionOptions()
        {
            if (!TryGetEntryUI("ambient_occlusion", out var ui)) return;

            ui.Setting = new ShadowResolutionSetting();
            var dropdown = (ui as SettingEntryDropdownUI).Dropdown;
            var options = new List<TMP_Dropdown.OptionData>();

            foreach (var value in Enum.GetValues(typeof(ShadowResolution)))
            {
                options.Add(new(text: ((Enum) value).ToReadable()));
            }

            dropdown.ClearOptions();
            dropdown.AddOptions(options);
        }

        void GetShadowResolutionOptions()
        {
            if (!TryGetEntryUI("shadow_resolution", out var ui)) return;

            ui.Setting = new ShadowResolutionSetting();
            var dropdown = (ui as SettingEntryDropdownUI).Dropdown;
            var options = new List<TMP_Dropdown.OptionData>();

            foreach (var value in Enum.GetValues(typeof(ShadowResolution)))
            {
                options.Add(new(text: ((Enum) value).ToReadable()));
            }

            dropdown.ClearOptions();
            dropdown.AddOptions(options);
        }

        #endregion


        #region Accessibility Settings

        void GetLanguageOptions()
        {
            if (!TryGetEntryUI("language", out var ui)) return;

            ui.Setting = new LanguageSetting();
            var dropdown = (ui as SettingEntryDropdownUI).Dropdown;
            var options = new List<TMP_Dropdown.OptionData>();

            foreach (var locale in Game.Locale.AvailableLocales)
            {
                options.Add(new(text: locale.DisplayName));
            }

            dropdown.ClearOptions();
            dropdown.AddOptions(options);
            dropdown.value = Game.Locale.AvailableLocales.IndexOf(Game.Locale.CurrentLocale);
        }

        #endregion


        bool TryGetEntryUI(string key, out SettingEntryUI ui)
        {
            if (userInterface.settingEntryUIs.TryGetValue(key, out ui))
            {
                return true;
            }
            else
            {
                Debug.LogError($"SettingEntryUI is missing for '{key}'");
                return false;
            }
        }
    }
}