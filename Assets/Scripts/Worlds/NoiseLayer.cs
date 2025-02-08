using System;

using UnityEngine;

namespace UZSG
{
    [Serializable]
    public class NoiseLayer
    {
        public int Seed;
        public NoiseType NoiseType;
        public Vector2 Offset;
        public int Octaves;
        [Range(0, 1)] public float Persistence;
        public float Lacunarity;
        public float Scale;
        [Range(0, 1)] public float Density;
    }
}