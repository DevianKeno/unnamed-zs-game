using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace UZSG.Data
{
    [Serializable]
    [CreateAssetMenu(fileName = "New Entity Data", menuName = "UZSG/Entity/Entity Data")]
    public class EntityData : BaseData
    {
        [Header("Entity Data")]
        public AssetReference AssetReference;
        public string Name;
        
        [Header("Audio Data")]
        public AudioAssetsData AudioAssetsData;
    }
}
