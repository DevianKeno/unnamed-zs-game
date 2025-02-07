using Unity.Mathematics;
using UnityEngine;

namespace UZSG
{
    public static class Noise
    {
        public const float MIN_SCALE = 0.0001f;
        public const float MIN_RAND = -100000f;
        public const float MAX_RAND = 100000f;

        public static float[,] generate2DRandom01(int seed, int width, int height, float2 offset, float density01, float scale)
        {
            float[,] noiseMap = new float[width, height];
            var rand = Unity.Mathematics.Random.CreateFromIndex((uint)seed);
            var seedOffset = new float2(
                rand.NextFloat(MIN_RAND, MAX_RAND),
                rand.NextFloat(MIN_RAND, MAX_RAND));
            density01 = math.clamp(density01, 0, 1);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float2 samplePos = new float2(x, y) + (seedOffset + offset);
                    uint hash = math.asuint(samplePos.x * 73856093 + samplePos.y * 19349663 + seed);
                    var cellRand = Unity.Mathematics.Random.CreateFromIndex(hash);
                    float randomValue = cellRand.NextFloat(0f, 1f);
                    
                    noiseMap[x, y] = (randomValue < density01) ? 1f : 0f;
                }
            }

            return noiseMap;
        }

        public static float[,] generate2DSimplex(int seed, int width, int height, Vector2 offset, float scale)
        {
            float[,] noiseMap = new float[width, height];
            var rand = Unity.Mathematics.Random.CreateFromIndex((uint)seed);
            var seedOffset = new float2(
                rand.NextFloat(MIN_RAND, MAX_RAND),
                rand.NextFloat(MIN_RAND, MAX_RAND));
            scale = math.clamp(scale, MIN_SCALE, scale);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float sx = (x + seedOffset.x + offset.x) * scale;
                    float sy = (y + seedOffset.y + offset.y) * scale;

                    noiseMap[x, y] = noise.snoise(new float2(sx, sy));
                }
            }

            return noiseMap;
        }

        public static float[,] generate2DPerlin(
            int seed,
            int width, int height,
            float2 offset,
            int octaves, float persistence, float lacunarity,
            float scale)
        {
            var noiseMap = new float[width, height];
            var rand = Unity.Mathematics.Random.CreateFromIndex((uint)seed);
            var seedOffset = new Vector2(
                rand.NextFloat(MIN_RAND, MAX_RAND),
                rand.NextFloat(MIN_RAND, MAX_RAND));
            scale = math.clamp(scale, MIN_SCALE, scale);
            float minnest = 0f;
            float maxxest = 0f;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float amplitude = 1f;
                    float frequency = 1f;
                    float val = 0f;

                    for (int oct = 0; oct < octaves; oct++)
                    {
                        float sampleX = (x + offset.x + seedOffset.x) / (scale * frequency);
                        float sampleZ = (y + offset.y + seedOffset.y) / (scale * frequency);

                        var p = Mathf.PerlinNoise(sampleX, sampleZ) * 2 - 1;
                        val += p * amplitude;

                        amplitude *= persistence;
                        frequency *= lacunarity;
                    }

                    minnest = math.min(minnest, val);
                    maxxest = math.max(maxxest, val);
                    noiseMap[x, y] = val *= amplitude;
                }
            }

            // normalization
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    var invlerp = (noiseMap[x, y] - minnest) / (maxxest - minnest);
                    noiseMap [x, y] = math.clamp(invlerp, 0, 1);
                }
            }

            return noiseMap;
        }
    }
}