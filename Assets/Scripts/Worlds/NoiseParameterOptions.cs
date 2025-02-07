using System;
using UnityEngine;

namespace UZSG.Worlds
{
    [Serializable]
    public struct NoiseParameters
    {
        public NoiseType NoiseType;
        public int Seed;
        public Vector2 Offset;
        public int Octaves;
        [Range(0, 1)] public float Persistence;
        public float Lacunarity;
        public float Scale;
        [Range(0, 1)] public float Density;

        public void SetValues(NoiseParameters p)
        {
            this.Seed = p.Seed;
            this.Offset = p.Offset;
            this.Octaves = p.Octaves;
            this.Persistence = p.Persistence;
            this.Lacunarity = p.Lacunarity;
            this.Scale = p.Scale;
            this.Density = p.Density;
        }
    }
}