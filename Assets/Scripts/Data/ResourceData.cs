using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.AddressableAssets;

using UZSG.Attributes;

namespace UZSG.Data
{
    [Serializable]
    public struct ResourceInstances
    {
        public string Name;
        public AssetReference Model;
        public List<AttributeData> Attributes;
    }

    [Serializable]
    [CreateAssetMenu(fileName = "New Resource Object Data", menuName = "UZSG/Objects/Resource Object Data")]
    public class ResourceData : ObjectData
    {
        [Header("Resource")]
        public List<ResourceInstances> Instances;
    }
}