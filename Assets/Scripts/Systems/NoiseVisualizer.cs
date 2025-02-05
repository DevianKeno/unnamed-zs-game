#if UNITY_EDITOR
using System;
using UnityEditor;
#endif
using UnityEngine;
using UZSG.Worlds;

namespace UZSG
{
    public enum NoiseType {
        Random, Perlin
    }
    
    [ExecuteAlways]
    public class NoiseVisualizer : MonoBehaviour
    {
        public NoiseType NoiseType = NoiseType.Perlin;
        public int Seed;
        [Range(0, 1)] public float Density;
        [Header("Noise Settings")]
        public NoiseParameters noiseParameters;
        public Vector2Int Size = new(100, 100);
        public float Scale = 10f;
        public Vector2 Offset;

        bool needsUpdate = true;
        Texture2D noiseTexture;
        
        [Header("Visualization Settings")]
        [SerializeField] float distanceFromCamera = 2f;

        [SerializeField] MeshFilter meshFilter;
        [SerializeField] MeshRenderer meshRenderer;

        void OnEnable()
        {
            GenerateTexture();
        }

        void OnValidate()
        {
            GenerateTexture();
            if (SceneView.lastActiveSceneView != null)
            {
                PositionVisualizer(SceneView.lastActiveSceneView.camera.transform);
            }
        }

    //     void Update()
    //     {
    //         if (!Application.isPlaying)
    //         {
    // #if UNITY_EDITOR
    //             if (SceneView.lastActiveSceneView != null)
    //             {
    //             }
    // #endif
    //             if (needsUpdate)
    //             {
    //                 needsUpdate = false;
    //             }
    //         }
    //     }

        void GenerateTexture()
        {
            float[,] noiseMap = NoiseType switch
            {
                NoiseType.Random => Noise.Generate2DRandom01(Seed, width: Size.x, height: Size.y, density01: Density / 100, Offset),
                NoiseType.Perlin => Noise.Generate2DPerlin(Seed, width: Size.x, height: Size.y, Offset,
                    octaves: noiseParameters.Octaves,
                    persistence: noiseParameters.Lacunarity,
                    lacunarity: noiseParameters.Lacunarity,
                    scale: noiseParameters.NoiseScale),
                _ => throw new NotImplementedException(),
            };

            noiseTexture = new Texture2D(width: Size.x, height: Size.y);
    
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
            meshFilter.mesh = CreateQuad();
            meshRenderer.sharedMaterial.SetTexture("_MainTex", noiseTexture);
        }

        Mesh CreateQuad()
        {
            Mesh mesh = new Mesh();
            mesh.vertices = new Vector3[]
            {
                new Vector3(-1, -1, 0), new Vector3(1, -1, 0),
                new Vector3(-1, 1, 0), new Vector3(1, 1, 0)
            };
            mesh.uv = new Vector2[]
            {
                new Vector2(0, 0), new Vector2(1, 0),
                new Vector2(0, 1), new Vector2(1, 1)
            };
            mesh.triangles = new int[] { 0, 2, 1, 2, 3, 1 };
            mesh.RecalculateNormals();
            return mesh;
        }

        void PositionVisualizer(Transform cameraTransform)
        {
            transform.position = cameraTransform.position + cameraTransform.forward * distanceFromCamera;
            transform.rotation = Quaternion.LookRotation(cameraTransform.forward);
        }

        public void MarkDirty()
        {
            needsUpdate = true;
        }
    }
}