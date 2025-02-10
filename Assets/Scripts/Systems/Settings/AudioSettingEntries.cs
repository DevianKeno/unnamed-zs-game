using System;

using UnityEngine;

namespace UZSG.Settings
{
    #region Audio Settings

    public class MasterVolumeSetting : SettingEntry
    {
        [Range(0, 1)] public float Value;

        public override void Load()
        {
            Value = PlayerPrefs.GetFloat("master_volume", 1f);
            Apply(Value);
        }

        public override void Save()
        {
            PlayerPrefs.SetFloat("master_volume", Mathf.Clamp01(Value));
        }

        public override void Apply(object value)
        {
            if (value is float valueFloat)
            {
                Game.Audio.MasterVolume = valueFloat;  
            }
        }
    }

    public class MusicVolumeSetting : SettingEntry
    {
        [Range(0, 1)] public float Value;

        public override void Apply(object value)
        {
            if (value is float valueFloat)
            {
                Game.Audio.MusicVolume = valueFloat;
            }
        }

        public override void Load()
        {
            Value = PlayerPrefs.GetFloat("music_volume", 1f);
        }

        public override void Save()
        {
            PlayerPrefs.SetFloat("music_volume", Mathf.Clamp01(Value));
            Apply(Value);
        }
    }

    public class SoundVolumeSetting : SettingEntry
    {
        [Range(0, 1)] public float Value;

        public override void Load()
        {
            Value = PlayerPrefs.GetFloat("sound_volume", 1f);
            Apply(Value);
        }

        public override void Save()
        {
            PlayerPrefs.SetFloat("sound_volume", Mathf.Clamp01(Value));
        }

        public override void Apply(object value)
        {
            if (value is float valueFloat)
            {
                Game.Audio.SoundVolume = valueFloat;
            }
        }
    }

    public class AmbianceVolumeSetting : SettingEntry
    {
        [Range(0, 1)] public float Value;

        public override void Apply(object value)
        {
            Game.Audio.ambianceVolume = Value;
            Apply(Value);
        }

        public override void Load()
        {
            Value = PlayerPrefs.GetFloat("ambiance_volume", 0.2f);
        }

        public override void Save()
        {
            PlayerPrefs.SetFloat("ambiance_volume", Mathf.Clamp01(Value));
        }
    }

    public enum SoundProfile {
        Stereo, Mono,
    }
    
    public class SoundProfileSetting : SettingEntry
    {
        public SoundProfile Profile;

        public override void Apply(object value)
        {
            
        }

        public override void Load()
        {
            Profile = (SoundProfile) PlayerPrefs.GetInt("sound_profile", 0);
        }

        public override void Save()
        {
            PlayerPrefs.SetInt("sound_profile", (int) Profile);
        }
    }
    
    #endregion
}