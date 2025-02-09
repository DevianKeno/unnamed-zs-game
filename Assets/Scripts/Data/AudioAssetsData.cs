using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.AddressableAssets;

namespace UZSG.Data
{
    /// <summary>
    /// Audio assets data.
    /// Values are set in Inspector; <b>Do not write</b>.
    /// </summary>
    [Serializable]
    [CreateAssetMenu(fileName = "New Audio Assets Data", menuName = "UZSG/Audio Assets Data")]
    public class AudioAssetsData : ScriptableObject
    {
        public int PoolSize;
        public string Path;
        public List<AssetReference> AudioClips;
    }
}