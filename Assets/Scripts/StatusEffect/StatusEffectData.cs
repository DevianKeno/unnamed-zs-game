using System;
using UnityEngine;
using UZSG.Data;

namespace UZSG.Data
{
    public enum StatusEffectType {
        Harmful, Beneficial
    }

    [Serializable]
    [CreateAssetMenu(fileName = "New Status Effect Data", menuName = "UZSG/Status Effect Data")]
    public class StatusEffectData : BaseData
    {
        public string DisplayName;
        public string DisplayNameTranslatable => Game.Locale.Translatable($"item.{Id}.name");
        [TextArea] public string Description;
        public string DescriptionTranslatable => Game.Locale.Translatable($"item.{Id}.description");
    }
}