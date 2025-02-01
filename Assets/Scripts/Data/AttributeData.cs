using System;

using UnityEngine;
using UnityEngine.Serialization;

namespace UZSG.Data
{
    [Serializable]
    [CreateAssetMenu(fileName = "Attribute", menuName = "UZSG/Attributes/Attribute Data")]
    public class AttributeData : BaseData
    {
        [Header("Attribute Data")]
        [FormerlySerializedAs("Name")] public string DisplayName;
        public string Group;
        [TextArea] public string Description;
    }
}