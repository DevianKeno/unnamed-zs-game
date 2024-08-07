using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.AddressableAssets;

namespace UZSG.Data
{
    [Serializable]
    [CreateAssetMenu(fileName = "New Audio Assets Data", menuName = "UZSG/Audio Assets Data")]
    public class AudioAssetsData : ScriptableObject
    {
        public string Path;
        public List<AssetReference> AudioClips;
    }
}