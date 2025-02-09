using System;

using UnityEngine;
using UnityEngine.Serialization;

namespace UZSG.Data
{
    /// <summary>
    /// Unique keys for localization.
    /// Values are set in Inspector; <b>Do not write</b>.
    /// </summary>
    [Serializable]
    [CreateAssetMenu(fileName = "New Translatable Key", menuName = "UZSG/Translatable Key")]
    public class TranslatableKey : BaseData
    {
        public string Key;
        [FormerlySerializedAs("Default")] public string DefaultText;
    }
}