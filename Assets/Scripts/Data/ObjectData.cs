using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.AddressableAssets;

namespace UZSG.Data
{
    [Serializable]
    [CreateAssetMenu(fileName = "New Object Data", menuName = "UZSG/Objects/Base Object Data")]
    public class ObjectData : BaseData
    {
        [Header("Object Data")]
        public string Name;
        public AssetReference Model;
        public List<Attributes.Attribute> Attributes;

        [Header("Audio Data")]
        public bool HasAudio = false;
        public AudioAssetsData AudioAssetsData;
    }
}