using System;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.UI;
using UZSG.Worlds;

namespace UZSG
{
    public enum NoiseType {
        Random, Simplex, Perlin
    }
    
    [ExecuteAlways]
    public class NoiseVisualizer : MonoBehaviour
    {
        public Vector2Int Size = new(100, 100);
        public NoiseParameters noiseParams;
        
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
            float[,] noiseMap = noiseParams.NoiseType switch
            {
                NoiseType.Random => Noise.generate2DRandom01(noiseParams.Seed, width: Size.x, height: Size.y,
                    offset: new Unity.Mathematics.float2(noiseParams.Offset.x, noiseParams.Offset.y),
                    density01: noiseParams.Density,
                    scale: noiseParams.Scale),

                NoiseType.Simplex => Noise.generate2DSimplex(noiseParams.Seed, width: Size.x, height: Size.y,
                    offset: new Unity.Mathematics.float2(noiseParams.Offset.x, noiseParams.Offset.y),
                    scale: noiseParams.Scale),

                NoiseType.Perlin => Noise.generate2DPerlin(noiseParams.Seed, width: Size.x, height: Size.y,
                    offset: noiseParams.Offset,
                    octaves: noiseParams.Octaves,
                    persistence: noiseParams.Lacunarity,
                    lacunarity: noiseParams.Lacunarity,
                    scale: noiseParams.Scale),

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