using System;
using System.Collections.Generic;

using UnityEngine;



namespace UZSG.Data
{
    public enum SettingCategory {
        Audio, Video, Graphics, Controls, Accessibility,
    }

    /// <summary>
    /// Settings entry data.
    /// Values are set in Inspector; <b>Do not write</b>.
    /// </summary>
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