using System;

using UnityEngine;
using UnityEngine.Serialization;

namespace UZSG.Data
{
    /// <summary>
    /// Attribute data.
    /// Values are set in Inspector; <b>Do not write</b>.
    /// </summary>
    [Serializable]
    [CreateAssetMenu(fileName = "Attribute", menuName = "UZSG/Attributes/Attribute Data")]
    public class AttributeData : BaseData
    {
        [Header("Attribute Data")]
        [FormerlySerializedAs("Name")] public string DisplayName;
        public string DisplayNameTranslatable => Game.Locale.Translatable($"attribute.{Id}.name");

        [TextArea] public string Description;
        public string DescriptionTranslatable => Game.Locale.Translatable($"attribute.{Id}.description");

        public string Group;
    }
}