using Unity.Mathematics;
using UnityEngine;

namespace UZSG
{
    public static class Noise
    {
        public static float MIN_SCALE = 0.0001f;

        public static float[,] generate2DRandom01(int seed, int width, int height, float2 offset, float density01, float scale)
        {
            float[,] noiseMap = new float[width, height];
            var rand = new System.Random(seed);
            var seedOffset = new float2(
                rand.Next(int.MinValue, int.MaxValue),
                rand.Next(int.MinValue, int.MaxValue)
            );
            float2 finalOffset = seedOffset + offset;
            density01 = math.clamp(density01, 0, 1);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int sampleX = (int) math.floor(x + finalOffset.x);
                    int sampleY = (int) math.floor(y + finalOffset.y);

                    /// what the fuck is this chat
                    int hash = (sampleX * 73856093) ^ (sampleY * 19349663) ^ seed;
                    var cellRand = new System.Random(hash);
                    int randomValue = cellRand.Next(0, 100);

                    noiseMap[x, y] = randomValue < (density01 * 100) ? 1 : 0;
                }
            }

            return noiseMap;
        }

        public static float[,] Generate2DRandom01(int seed, int width, int height, float density01, Vector2 offset)
        {
            float[,] noiseMap = new float[width, height];
            var rand = new System.Random(seed);
            var seedOffset = new Vector2(
                rand.Next(-100000, 100000),
                rand.Next(-100000, 100000)
            );
            Vector2 finalOffset = seedOffset + offset;
            density01 = Mathf.Clamp01(density01);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int sampleX = Mathf.FloorToInt(x + finalOffset.x);
                    int sampleY = Mathf.FloorToInt(y + finalOffset.y);

                    /// what the fuck is this chat
                    int hash = (sampleX * 73856093) ^ (sampleY * 19349663) ^ seed;
                    var cellRand = new System.Random(hash);
                    int randomValue = cellRand.Next(0, 100);

                    noiseMap[x, y] = randomValue < (density01 * 100) ? 1 : 0;
                }
            }

            return noiseMap;
        }

        public static float[,] Generate2DPerlin(int seed, int chunkSize, Vector2 offset, int octaves, float persistence, float lacunarity, float scale)
        {
            var noiseMap = new float[chunkSize, chunkSize];
            var rand = new System.Random(seed);
            var seedOffset = new Vector2(
                rand.Next(-100000, 100000),
                rand.Next(-100000, 100000)
            );

            scale = scale <= 0 ? MIN_SCALE : scale;

            for (int z = 0, i = 0; z < chunkSize; z++)
            {
                for (int x = 0; x < chunkSize; x++)
                {
                    float amplitude = 1f;
                    float frequency = 1f;

                    for (int oct = 0; oct < octaves; oct++)
                    {
                        float sampleX = (x + offset.x + seedOffset.x) / scale * frequency;
                        float sampleZ = (z + offset.y + seedOffset.y) / scale * frequency;

                        float perlinValue = Mathf.PerlinNoise(sampleX, sampleZ) * 2 - 1;
                        noiseMap[x, z]  =perlinValue *= amplitude;

                        amplitude *= persistence;
                        frequency *= lacunarity;
                    }

                    i++;
                }
            }

            return noiseMap;
        }

        public static float[,] Generate2DPerlin(int seed, int width, int height, Vector2 offset, int octaves, float persistence, float lacunarity, float scale)
        {
            var noiseMap = new float[width, height];
            var rand = new System.Random(seed);
            var seedOffset = new Vector2(
                rand.Next(-100000, 100000),
                rand.Next(-100000, 100000)
            );

            scale = scale <= 0 ? MIN_SCALE : scale;

            for (int y = 0, i = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float amplitude = 1f;
                    float frequency = 1f;

                    for (int oct = 0; oct < octaves; oct++)
                    {
                        float sampleX = (x + offset.x + seedOffset.x) / scale * frequency;
                        float sampleZ = (y + offset.y + seedOffset.y) / scale * frequency;

                        float perlinValue = Mathf.PerlinNoise(sampleX, sampleZ) * 2 - 1;
                        noiseMap[x, y]  =perlinValue *= amplitude;

                        amplitude *= persistence;
                        frequency *= lacunarity;
                    }

                    i++;
                }
            }

            return noiseMap;
        }
    }
}