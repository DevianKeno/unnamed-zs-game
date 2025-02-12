using System;
using System.Collections.Generic;

using UnityEngine;

namespace UZSG.Data
{
    /// <summary>
    /// Manifest of localization data for locales.
    /// Values are set in Inspector; <b>Do not write</b>.
    /// </summary>
    [Serializable]
    [CreateAssetMenu(fileName = "New Localization Data", menuName = "Localization Data")]
    public class LocalizationData : BaseData
    {
        public string DisplayName;
        public string LocaleKey;
        public Dictionary<string, string> Translations;
    }
}