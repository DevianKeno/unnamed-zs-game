using System;

using UnityEngine;

namespace UZSG
{
    /// <summary>
    /// Represents an image displayed on the Settings Information Window when a specific option is selected.
    /// </summary>
    [Serializable]
    public struct SettingImageDisplay
    {
        public SettingsQualityFlags Quality;
        public Sprite Image;
    }
}