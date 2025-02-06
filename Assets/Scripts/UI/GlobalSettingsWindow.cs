using System.Collections.Generic;

using UnityEngine;
using UnityEngine.EventSystems;
using static UnityEngine.EventSystems.PointerEventData.InputButton;

namespace UZSG.UI
{
    public class GlobalSettingsWindow : Window
    {
        internal Dictionary<string, SettingEntryUI> settingEntryUIs = new();

        [SerializeField] SettingInformationDisplayUI settingInformationDisplay;

        internal void Initialize()
        {
            var allEntries = GetComponentsInChildren<SettingEntryUI>();
            foreach (SettingEntryUI settingEntryUI in allEntries)
            {
                if (settingEntryUI.Data == null)
                {
                    Debug.LogWarning($"No setting set for SettingEntryUI '{settingEntryUI.name}'!");
                    continue;
                }

                settingEntryUIs[settingEntryUI.Data.Id] = settingEntryUI;
                settingEntryUI.OnClicked += OnSettingEntryMouseDown;
                settingEntryUI.OnValueChanged += OnSettingValueChanged;
            }
        }
        
        
        #region Event callbacks

        void OnSettingEntryMouseDown(object sender, PointerEventData click)
        {
            var settingEntry = (SettingEntryUI) sender;

            settingInformationDisplay.SetSettingData(settingEntry.Data);
        }

        void OnSettingValueChanged(SettingEntryUI settingEntry)
        {        
        }

        #endregion
    }
}