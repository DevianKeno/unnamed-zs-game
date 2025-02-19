using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using TMPro;

using UZSG.UI;
using UZSG.Data;
using UZSG.Settings;

namespace UZSG
{
    public class SettingsManager : MonoBehaviour
    {
        bool _isInitialized = false;
        Dictionary<string, SettingEntry> settingsEntries = new();
        Dictionary<SettingCategory, List<SettingsEntryData>> settingsDataDict = new();

        GlobalSettingsWindow userInterface;
        public GlobalSettingsWindow UI => userInterface;
        [SerializeField] GameObject globalSettingsWindow;

        internal void Initialize()
        {
            if (_isInitialized) return;

            _isInitialized = true;
            
            foreach (var setting in Resources.LoadAll<SettingsEntryData>("Data/Settings"))
            {
                if (!settingsDataDict.TryGetValue(setting.Category, out var list))
                {
                    list = new();
                    settingsDataDict[setting.Category] = list;
                }
                
                list.Add(setting);
            }

            userInterface = Instantiate(globalSettingsWindow, parent: Game.UI.Canvas.transform).GetComponent<GlobalSettingsWindow>();
            userInterface.name = "Global Settings Window";
            userInterface.Initialize();
            userInterface.Hide();


            LoadSettings();
        }

        public SettingEntry GetSetting(string id)
        {
            if (settingsEntries.TryGetValue(id, out var setting))
            {
                return setting;
            }
            return null;
        }


        #region Public methods

        /// <summary>
        /// Loads all settings stored in PlayerPrefs and applies them.
        /// </summary>
        public void LoadSettings()
        {
            LoadAudioSettings();
            LoadVideoSettings();
            LoadGraphicsSettings();
            // LoadControlsSettings();
            LoadAccessibilitySettings();
        }

        /// <summary>
        /// Reverts all applied settings back to its original loaded value.
        /// </summary>
        public void RevertSettings()
        {
            foreach (SettingCategory category in Enum.GetValues(typeof(SettingCategory)))
            {
                if (false == settingsDataDict.TryGetValue(category, out var settingList))
                {
                    Debug.LogWarning($"{category} was not found in settings dictionary! Is it empty?");
                    continue;
                }

                foreach (var setting in settingList)
                {
                    if (false == TryGetUIEntry(setting.Id, out SettingEntryUI settingUI) ||
                        false == settingUI.IsDirty)
                    {
                        continue;
                    }
                    
                    if (settingUI.Setting == null)
                    {
                        Debug.LogWarning($"SettingEntry for '{setting.Id}' is null!");
                        return;
                    }

                    settingUI.Setting.Revert();
                }
            }
        }

        /// <summary>
        /// Applies the current settings, but does not save them.
        /// </summary>
        public void ApplySettings()
        {
            foreach (SettingCategory category in Enum.GetValues(typeof(SettingCategory)))
            {
                if (false == settingsDataDict.TryGetValue(category, out var settingList))
                {
                    Debug.LogWarning($"{category} was not found in settings dictionary! Is it empty?");
                    continue;
                }

                foreach (var setting in settingList)
                {
                    if (false == TryGetUIEntry(setting.Id, out SettingEntryUI settingUI) ||
                        false == settingUI.IsDirty)
                    {
                        continue;
                    }
                    
                    if (settingUI.Setting == null)
                    {
                        Debug.LogWarning($"SettingEntry for '{setting.Id}' is null!");
                        return;
                    }

                    settingUI.Setting.Apply(settingUI.Value);
                }
            }
        }

        /// <summary>
        /// Saves all current settings stores it in PlayerPrefs.
        /// </summary>
        public void SaveSettings()
        {
            foreach (SettingCategory category in Enum.GetValues(typeof(SettingCategory)))
            {
                if (false == settingsDataDict.TryGetValue(category, out var settingList))
                {
                    Debug.LogWarning($"{category} was not found in settings dictionary! Is it empty?");
                    continue;
                }

                foreach (var setting in settingList)
                {
                    if (false == TryGetUIEntry(setting.Id, out SettingEntryUI settingUI) ||
                        false == settingUI.IsDirty)
                    {
                        continue;
                    }
                    
                    if (settingUI.Setting == null)
                    {
                        Debug.LogWarning($"SettingEntry for '{setting.Id}' is null!");
                        return;
                    }

                    settingUI.Setting.Apply(settingUI.Value);
                    settingUI.Setting.Save();
                }
            }

            PlayerPrefs.Save();
        }

        /// <summary>
        /// Shows the Global Settings Window.
        /// </summary>
        public void ShowGlobalInterface()
        {
            userInterface.Show();
        }

        /// <summary>
        /// Hide the Global Settings Window.
        /// </summary>
        public void HideGlobalInterface()
        {
            userInterface.Hide();
        }

        /// <summary>
        /// Retrieve the UI element tied to a setting.
        /// </summary>
        public bool TryGetUIEntry(string key, out SettingEntryUI ui)
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

        [ContextMenu("Force Save All Settings")]
        public void ForceSaveGlobalSettings()
        {
            foreach (SettingCategory category in Enum.GetValues(typeof(SettingCategory)))
            {
                foreach (var setting in settingsDataDict[category])
                {
                    if (false == TryGetUIEntry(setting.Id, out SettingEntryUI ui))
                    {
                        continue;
                    }
                    
                    if (ui.Setting == null)
                    {
                        Debug.LogWarning($"SettingEntry for '{setting.Id}' is null!");
                        return;
                    }

                    ui.Setting.Apply(ui.Value);
                    ui.Setting.Save();
                }
            }

            PlayerPrefs.Save();
        }

        #endregion


        void LoadAudioSettings()
        {
            
        }

        void LoadVideoSettings()
        {
            GetMonitorOptions();
            GetFullscreenModeOptions();
            GetResolutionOptions();
            GetVSyncOptions();
            GetFramerateCapOptions();
        }

        void LoadGraphicsSettings()
        {
            
        }

        void LoadControlsSettings()
        {
            
        }

        void LoadAccessibilitySettings()
        {
            GetLanguageOptions();
        }
        

        #region Audio  Settings



        #endregion

    
        #region Video Settings

        void GetMonitorOptions()
        {
            if (!TryGetUIEntry(SettingId.Monitor, out var ui)) return;
            
            var monitorSetting = new MonitorSetting
            {
                UI = ui
            };
            monitorSetting.Load();
            ui.Setting = monitorSetting;
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
        
        void GetFullscreenModeOptions()
        {
            if (!TryGetUIEntry(SettingId.FullscreenMode, out var ui)) return;

            var windowModeSetting = new WindowModeSetting
            {
                UI = ui
            };
            windowModeSetting.Load();
            ui.Setting = windowModeSetting;

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
            if (!TryGetUIEntry("resolution", out var ui)) return;
            
            var availableResolutions = Screen.resolutions.Distinct().ToArray();
            var options = new List<string>();

            var resolutionSetting = new ResolutionSetting();
            resolutionSetting.Load();
            ui.Setting = resolutionSetting;

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
            if (!TryGetUIEntry("v_sync", out var ui)) return;

            var vSyncSetting = new VSyncSetting();
            vSyncSetting.Load();
            ui.Setting = vSyncSetting; 

            var toggle = (ui as SettingEntryToggleUI).Toggle;
            toggle.isOn = QualitySettings.vSyncCount > 0;
        }

        void GetFramerateCapOptions()
        {
            if (!TryGetUIEntry("framerate_cap", out var ui)) return;

            var framerateCapSetting = new FramerateCapSetting();
            framerateCapSetting.Load();
            ui.Setting = framerateCapSetting;

            var slider = (ui as SettingEntrySliderUI).Slider;
            slider.minValue = Mathf.FloorToInt((float) FramerateCapSetting.MIN_FPS);
            slider.maxValue = Mathf.FloorToInt((float) Screen.currentResolution.refreshRateRatio.value) + 1; /// +1 for no cyap 
            slider.value = Application.targetFrameRate < 0 ? slider.maxValue : slider.value;
        }

        #endregion


        #region Graphics Settings

        void GetAmbientOcclusionOptions()
        {
            if (!TryGetUIEntry("ambient_occlusion", out var ui)) return;

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
            if (!TryGetUIEntry("shadow_resolution", out var ui)) return;

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
            if (!TryGetUIEntry("language", out var ui)) return;

            var languageSetting = new LanguageSetting();
            languageSetting.Load();
            ui.Setting = languageSetting;

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
    }
}