using System;

using UnityEngine;

namespace UZSG.Settings
{
    #region Accesisbility Settings

    public class LanguageSetting : SettingEntry
    {
        public string LocaleKey;

        public override void Load()
        {
            LocaleKey = PlayerPrefs.GetString("language", "en_us");
        }

        public override void Save()
        {
            PlayerPrefs.SetString("language", Game.Locale.CurrentLocale.LocaleKey);
        }
        
        public override void Apply(object value)
        {
            if (value is string localeKey)
            {
                Game.Locale.SetLocalization(localeKey);
            }
        }
    }

    #endregion
}