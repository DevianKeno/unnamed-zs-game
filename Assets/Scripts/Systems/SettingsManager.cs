using System;
using System.Collections.Generic;

using UnityEngine;
using TMPro;

using UZSG.UI;

namespace UZSG.Systems
{
    public class SettingsManager : MonoBehaviour
    {
        GlobalSettingsWindow userInterface;
        [SerializeField] GameObject globalSettingsWindow;

        internal void Initialize()
        {
            userInterface = Instantiate(globalSettingsWindow, parent: Game.UI.Canvas.transform).GetComponent<GlobalSettingsWindow>();
            userInterface.name = "Global Settings Window";
            userInterface.Initialize();
            userInterface.Hide();

            SetLocalizations();

            LoadGlobalSettings();
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
            /// Shadows Settings
            PlayerPrefs.SetInt("shadows_quality", (int) QualitySettings.shadowResolution);
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
            PlayerPrefs.SetString("language", Game.Locale.CurrentLocale.LocaleKey);
            if (TryGetEntryUI("language", out var ui))
            {
                ((SettingEntryDropdownUI) ui).SetSelected(3); /// TODO:
            }
        }

        void SetLocalizations()
        {
            if (false == TryGetEntryUI("language", out var ui))
            {
                Debug.LogError($"Setting entry ui is missing for localization");
                return;
            }

            var dropdown = (ui as SettingEntryDropdownUI).Dropdown;
            var options = new List<TMP_Dropdown.OptionData>();
            foreach (var locale in Game.Locale.AvailableLocales)
            {
                options.Add(new(text: locale.DisplayName));
            }
            dropdown.ClearOptions();
            dropdown.AddOptions(options);
        }

        bool TryGetEntryUI(string key, out SettingEntryUI ui)
        {
            return userInterface.settingEntryUIs.TryGetValue(key, out ui);
        }
    }
}