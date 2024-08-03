using System;

using UnityEngine;

namespace UZSG.Data
{
    [Serializable]
    [CreateAssetMenu(fileName = "Attribute", menuName = "UZSG/Attributes/Attribute Data")]
    public class AttributeData : BaseData
    {
        public string Name;
        [TextArea] public string Description;
        public Attributes.Type Type;
    }
}