using System;

using UnityEngine;

namespace UZSG.Settings
{
    #region Graphics Settings

    public class ShadowResolutionSetting : SettingEntry
    {
        public ShadowResolution Resolution;
        
        public override void Load()
        {
            var resInt = PlayerPrefs.GetInt("shadow_resolution", 1);
            Resolution = (ShadowResolution) resInt;
            Apply(resInt);
        }

        public override void Save()
        {
            PlayerPrefs.SetInt("shadow_resolution", (int) Resolution);
        }

        public override void Apply(object value)
        {
            if (value is int valueInt)
            {
                if (Enum.IsDefined(typeof(ShadowResolution), valueInt))
                {
                    QualitySettings.shadowResolution = (ShadowResolution) valueInt;
                }
            }
        }
    }

    #endregion
}