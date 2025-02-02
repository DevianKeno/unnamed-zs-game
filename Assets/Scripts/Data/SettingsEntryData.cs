using System;
using System.Collections.Generic;

using UnityEngine;

using UZSG.Systems;

namespace UZSG.Data
{
    [Serializable]
    [CreateAssetMenu(fileName = "New Setting Entry Data", menuName = "UZSG/Setting Entry Data")]
    public class SettingsEntryData : BaseData
    {
        [SerializeField] internal string displayName;
        public string DisplayName
        {
            get => Game.Locale.Translatable($"setting.{Id}.name");
        }
        [SerializeField, TextArea] internal string description;
        public string Description
        {
            get => Game.Locale.Translatable($"setting.{Id}.description");
        }
        public SettingsQualityFlags PerformanceImpact;
        /// <summary>
        /// Add images displayed on the Settings Information Window when a specific option is selected.
        /// </summary>
        public List<SettingImageDisplay> Images;
    }
}