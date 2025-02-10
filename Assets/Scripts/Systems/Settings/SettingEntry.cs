using System;

using UnityEngine;

namespace UZSG
{
    public abstract class SettingEntry
    {
        /// <summary>
        /// Loads the setting value from PlayerPrefs and applies it immediately.
        /// </summary>
        public virtual void Load() { }
        /// <summary>
        /// Saves the setting value to PlayerPrefs.
        /// </summary>
        public virtual void Save() { }
        /// <summary>
        /// Applies the setting.
        /// </summary>
        public virtual void Apply(object value) { }
        /// <summary>
        /// Reverts to the original setting.
        /// </summary>
        public virtual void Revert() { }
    }
    
    public enum SettingQualityValue {
        VeryLow, Low, Medium, High, VeryHigh, Ultra
    }
}