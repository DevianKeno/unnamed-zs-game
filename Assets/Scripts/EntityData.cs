using System;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace UZSG.Entities
{
    [Serializable]
    [CreateAssetMenu(fileName = "EntityData", menuName = "URMG/Entity Data")]
    public class EntityData : ScriptableObject
    {
        public string Id;
        public string Name;
        public AssetReference AssetReference;
    }
}
