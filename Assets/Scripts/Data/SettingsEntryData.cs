using System;
using System.Collections.Generic;

using UnityEngine;

using UZSG.Systems;

namespace UZSG.Data
{
    public enum SettingCategory {
        Audio, Video, Graphics, Controls, Accessibility,
    }

    [Serializable]
    [CreateAssetMenu(fileName = "New Setting Entry Data", menuName = "UZSG/Setting Entry Data")]
    public class SettingsEntryData : BaseData
    {
        public SettingCategory Category;
        [SerializeField] internal string displayName;
        public string DisplayNameTranslatable => Game.Locale.Translatable($"setting.{Id}.name");
        [SerializeField, TextArea] internal string description;
        public string DescriptionTranslatable => Game.Locale.Translatable($"setting.{Id}.description");
        public SettingsQualityFlags PerformanceImpact;
        /// <summary>
        /// Add images displayed on the Settings Information Window when a specific option is selected.
        /// </summary>
        public List<SettingImageDisplay> Images;
    }
}