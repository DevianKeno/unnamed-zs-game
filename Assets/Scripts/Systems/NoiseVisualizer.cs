using System;

using UnityEngine;
using UnityEngine.UI;

using UZSG.Data;

namespace UZSG
{
    [ExecuteAlways]
    public class NoiseVisualizer : MonoBehaviour
    {
        public Vector2Int Size = new(100, 100);
        public NoiseLayer noiseLayer;
        
        [SerializeField] Image image;

        void OnEnable()
        {
            GenerateTexture();
        }

        void OnValidate()
        {
            GenerateTexture();
        }

        void GenerateTexture()
        {
            float[,] noiseMap = noiseLayer.NoiseType switch
            {
                NoiseType.Random => Noise.generate2DRandom01(noiseLayer.Seed, width: Size.x, height: Size.y,
                    offset: new Unity.Mathematics.float2(noiseLayer.Offset.x, noiseLayer.Offset.y),
                    density01: noiseLayer.Density,
                    scale: noiseLayer.Scale),

                NoiseType.Simplex => Noise.generate2DSimplex(noiseLayer.Seed, width: Size.x, height: Size.y,
                    offset: new Unity.Mathematics.float2(noiseLayer.Offset.x, noiseLayer.Offset.y),
                    scale: noiseLayer.Scale),

                NoiseType.Perlin => Noise.generate2DPerlin(noiseLayer.Seed, width: Size.x, height: Size.y,
                    offset: noiseLayer.Offset,
                    octaves: noiseLayer.Octaves,
                    persistence: noiseLayer.Lacunarity,
                    lacunarity: noiseLayer.Lacunarity,
                    scale: noiseLayer.Scale),

                _ => throw new NotImplementedException(),
            };

            var noiseTexture = new Texture2D(width: Size.x, height: Size.y);
    
            for (int y = 0; y < noiseMap.GetLength(1); y++)
            {
                for (int x = 0; x < noiseMap.GetLength(0); x++)
                {
                    var value = noiseMap[x, y];
                    var color = new Color(value, value, value);
                    noiseTexture.SetPixel(x, y, color);
                }
            }
            noiseTexture.Apply();
            image.sprite = Sprite.Create(noiseTexture, new Rect(0, 0, Size.x,  Size.y), Vector2.one * 0.5f);
        }
    }
}