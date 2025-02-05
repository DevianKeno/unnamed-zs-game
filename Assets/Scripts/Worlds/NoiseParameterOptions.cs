using System;
using UnityEngine;

namespace UZSG.Worlds
{
    [Serializable]
    public struct NoiseParameters
    {
        public NoiseType Type;
        public int Seed;
        public Vector2 Offset;
        public int Octaves;
        public float Persistence;
        public float Lacunarity;
        public float NoiseScale;
        public float Threshold;

        public void SetValues(NoiseParameters p)
        {
            this.Seed = p.Seed;
            this.Offset = p.Offset;
            this.Octaves = p.Octaves;
            this.Persistence = p.Persistence;
            this.Lacunarity = p.Lacunarity;
            this.NoiseScale = p.NoiseScale;
            this.Threshold = p.Threshold;
        }
    }
}