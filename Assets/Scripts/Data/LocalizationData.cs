using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Serialization;

namespace UZSG.Data
{
    [Serializable]
    [CreateAssetMenu(fileName = "New Localization Data", menuName = "Localization Data")]
    public class LocalizationData : ScriptableObject
    {
        public Dictionary<string, string> Translations;
    }
}