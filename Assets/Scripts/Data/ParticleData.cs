using System;
using System.Collections.Generic;

using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;

using UZSG.Crafting;
using UZSG.Data;

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