using System;

using UnityEngine;

namespace UZSG.Settings
{
    #region Accesisbility Settings

    public class LanguageSetting : SettingEntry
    {
        public override void Load()
        {
            var localeKey = PlayerPrefs.GetString("language", "en_us");
            Apply(Game.Locale.GetIndexOf(localeKey));
        }

        public override void Save()
        {
            PlayerPrefs.SetString("language", Game.Locale.CurrentLocale.LocaleKey);
        }
        
        public override void Apply(object value)
        {
            if (value is int localeIndex)
            {
                if (Game.Locale.AvailableLocales.IsValidIndex(localeIndex))
                {
                    ChangeLocaleAsync(localeIndex);
                }
            }
        }

        public override void Revert()
        {
            var localeKey = PlayerPrefs.GetString("language", "en_us");
            Apply(Game.Locale.GetIndexOf(localeKey));
        }

        async void ChangeLocaleAsync(int localeIndex)
        {
            var isSuccessful = await Game.Locale.SetLocalization(Game.Locale.AvailableLocales[localeIndex].LocaleKey);
            if (isSuccessful)
            {
                if (Game.Settings.TryGetUIEntry(SettingId.Language, out var ui))
                {
                    ui.SetValue<int>(Game.Locale.GetIndexOf(Game.Locale.CurrentLocale.LocaleKey));
                }
            }
        }
    }

    #endregion
}