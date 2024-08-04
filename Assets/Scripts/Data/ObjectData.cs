using System;

using UnityEngine;
using UnityEngine.AddressableAssets;

using UZSG.Attributes;

namespace UZSG.Data
{
    [Serializable]
    [CreateAssetMenu(fileName = "New Object Data", menuName = "UZSG/Object Data")]
    public class ObjectData : BaseData
    {
        [Header("Object")]
        public string Name;
        public AssetReference Model;
        public AttributeCollection<Attributes.Attribute> Attributes;
    }
}