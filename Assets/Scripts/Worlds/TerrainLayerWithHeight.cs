using System;

using UnityEngine;

namespace UZSG.Worlds
{
    [Serializable]
    public struct TerrainLayerWithHeight
    {
        public TerrainLayer Layer;
        public float Height;
        [Range(0f, 1f)] public float Weight;
    }
}