using System;

using UnityEngine;

namespace UZSG.Data
{
    /// <summary>
    /// Noise data for procedural randomness.
    /// Values are set in Inspector; <b>Do not write</b>.
    /// </summary>
    [Serializable]
    [CreateAssetMenu(fileName = "New Noise Data", menuName = "UZSG/Noise Data")]
    public class NoiseData : BaseData
    {
        /// <summary>
        /// The noise pattern to be used.
        /// </summary>
        public NoiseLayer Noise;
        /// <summary>
        /// For resources. Whether the resource can only spawn in specific layer/s. 
        /// </summary>
        public bool TerrainLayerConstrained;
        /// <summary>
        /// Layers of terrain to spawn on.
        /// </summary>
        public TerrainLayer[] TerrainLayers;
    }
}