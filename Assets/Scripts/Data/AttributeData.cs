using System;

using UnityEngine;

namespace UZSG.Data
{
    [Serializable]
    [CreateAssetMenu(fileName = "Attribute", menuName = "UZSG/Attributes/Attribute Data")]
    public class AttributeData : BaseData
    {
        [Header("Attribute Data")]
        public string Name;
        public string Group;
        [TextArea] public string Description;
    }
}