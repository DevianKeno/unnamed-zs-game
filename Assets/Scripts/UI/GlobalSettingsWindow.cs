using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static UnityEngine.EventSystems.PointerEventData.InputButton;

namespace UZSG.UI
{
    public class GlobalSettingsWindow : Window
    {
        internal Dictionary<string, SettingEntryUI> settingEntryUIs = new();

        [Header("UI Elements")]
        [SerializeField] SettingInformationDisplayUI settingInformationDisplay;
        [SerializeField] Button applyButton;
        [SerializeField] Button saveButton;
        [SerializeField] Button closeButton;

        protected override void Awake()
        {
            base.Awake();
            applyButton.onClick.AddListener(OnApplyBtnClick);
            saveButton.onClick.AddListener(OnSaveBtnClick);
            closeButton.onClick.AddListener(OnCloseBtnClick);
        }
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
                settingEntryUI.OnMouseDown += OnSettingEntryMouseDown;
            }
        }
        
        void OnApplyBtnClick()
        {
            Game.Settings.ApplySettings();
        }

        void OnSaveBtnClick()
        {
            Game.Settings.SaveSettings();
            Hide();
        }

        void OnCloseBtnClick()
        {
            Game.Settings.RevertSettings();
            Hide();
        }

        
        #region Event callbacks

        void OnSettingEntryMouseDown(object sender, PointerEventData click)
        {
            var settingEntry = (SettingEntryUI) sender;

            settingInformationDisplay.SetSettingData(settingEntry.Data);
        }

        #endregion
    }
}