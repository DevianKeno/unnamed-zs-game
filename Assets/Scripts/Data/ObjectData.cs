using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Serialization;

namespace UZSG.Data
{
    [Serializable]
    [CreateAssetMenu(fileName = "New Object Data", menuName = "UZSG/Objects/Base Object Data")]
    public class ObjectData : BaseData
    {
        [Header("Object Data")]
        [FormerlySerializedAs("Name")] public string DisplayName;
        [FormerlySerializedAs("Name")]
        [TextArea] public string Description;
        [FormerlySerializedAs("Model")] public AssetReference Object;
        public List<Attributes.Attribute> Attributes;
        public bool CanBePickedUp;
        public float PickupTimeSeconds;

        [Header("Audio Data")]
        public bool HasAudio = false;
        public AudioAssetsData AudioAssetsData;

        public bool IsValid()
        {
            return Object != null && Object.IsSet();
        }
    }
}