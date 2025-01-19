using System;

using UnityEngine;
using UnityEngine.AddressableAssets;

namespace UZSG.Data
{
    [Serializable]
    [CreateAssetMenu(fileName = "New Particle Data", menuName = "UZSG/Particles/Particle Data")]
    public class ParticleData : BaseData
    {
        [Header("Particle Data")]
        public string Name;
        public AssetReference Asset;
    }
}