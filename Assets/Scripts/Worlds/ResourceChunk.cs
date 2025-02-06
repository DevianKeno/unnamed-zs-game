using System;
using System.Collections.Generic;

using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.AddressableAssets;

using UZSG.Objects;
using UZSG.Saves;

namespace UZSG.Worlds
{
    public class ResourceChunk : MonoBehaviour, ISaveDataReadWrite<ResourceChunkSaveData>
    {
        public Vector3Int Coord;
        [SerializeField] int seed;
        [SerializeField] NoiseParameters treeNoiseParams;
        [SerializeField] NoiseParameters pickupsNoiseParams;
        [SerializeField] GenerateResourceChunkSettings settings;

        int chunkSize;
        RaycastHit hit;
        /// <summary>
        /// List of resources populating this chunk.
        /// </summary>e
        [SerializeField] List<Resource> resources = new();

        /// <summary>
        /// The terrain this chunk is in.
        /// </summary>
        Terrain terrain;
        bool _isProcessing;
        JobHandle _currentJob;
        NativeArray<float> _noiseMap;

        void Update()
        {
            if (_isProcessing && _currentJob.IsCompleted)
            {
                _currentJob.Complete();
                PopulateChunk(_noiseMap);
            }
        }

        public void GenerateResources(int chunkSize, Vector3Int chunkCoord, GenerateResourceChunkSettings settings)
        {
            if (_isProcessing) return;

            _isProcessing = true;
            this.chunkSize = chunkSize;
            int groundLayerMask = LayerMask.NameToLayer("Ground");

            _noiseMap = new(chunkSize * chunkSize * chunkSize, Allocator.TempJob);
            CalculateNoiseRandom_Job calculateNoiseJob = new()
            {
                Seed = this.seed,
                Width = chunkSize,
                Height = chunkSize,
                Offset = new(chunkCoord.x, chunkCoord.y, chunkCoord.z),
                Density = settings.TreeDensity,
                Scale = 1f, /// TEST:
                NoiseMap = _noiseMap
            };

            _currentJob = calculateNoiseJob.Schedule(_noiseMap.Length, 8);
        }

        void PopulateChunk(NativeArray<float> noiseMap)
        {
            var groundLayerMask = LayerMask.NameToLayer("Ground");
            var worldPos = new Vector3(this.transform.position.x, 1000f, this.transform.position.z);
            Vector3 samplePoint;
            for (int i = 0; i < noiseMap.Length; i++)
            {
                int x = i % this.chunkSize;
                int y = (i / this.chunkSize) % this.chunkSize;
                int z = i / (this.chunkSize * this.chunkSize);
                float val = noiseMap[i];

                if (val > this.settings.TreeDensity &&
                    val > this.settings.PickupsDensity)
                {
                    continue;
                }
                /// sample world position relative to chunk
                samplePoint = new Vector3(
                    x + this.transform.position.x,
                    0f,
                    z + this.transform.position.x
                );

                // samplePoint.y = terrain.SampleHeight(samplePoint);
                // Handles.DrawWireCube(samplePoint, size: Vector3.one);
                // var layer = GetTerrainLayerFromPoint(terrain, samplePoint);

                if (val > settings.TreeDensity &&
                    settings.PlaceTrees)
                {
                    Debug.DrawRay(samplePoint, Vector3.up, Color.red, 1f);
                    // TryPlaceTree(layer, samplePoint);
                }

                if (val > settings.PickupsDensity &&
                    settings.PlacePickups)
                {
                    Debug.DrawRay(samplePoint, Vector3.up, Color.red, 1f);
                    // TryPlacePickup(layer, samplePoint);
                }
            }

            _noiseMap.Dispose();
        }

        /// <summary>
        /// Tries to place a Tree (Resource) at position given the terrain layer.
        /// </summary>
        void TryPlaceTree(TerrainLayer layer, Vector3 position)
        {
            if (layer.diffuseTexture.name.Equals("grass", StringComparison.OrdinalIgnoreCase))
            {
                Debug.DrawRay(position, Vector3.up, Color.green, 1f);
            }
            else
            {
                Debug.DrawRay(position, Vector3.up, Color.red, 1f);
            }
// #if UNITY_EDITOR
//                         PrefabUtility.InstantiatePrefab(null);
//                         // continue;
// #endif
//                         Addressables.LoadAssetAsync<>
                // Game.Objects.PlaceNew();
        }

        /// <summary>
        /// Tries to place a Resource Pickup at position given the terrain layer.
        /// </summary>
        void TryPlacePickup(TerrainLayer layer, Vector3 position)
        {

        }

        public void SetTreeNoiseParameters(NoiseParameters p)
        {
            treeNoiseParams.SetValues(p);
        }

        public void SetPickupNoiseParameters(NoiseParameters p)
        {
            pickupsNoiseParams.SetValues(p);
        }

        public void SetSeed(int seed)
        {
            this.seed = seed;
        }
        
        public void ReadSaveData(ResourceChunkSaveData saveData)
        {
            
        }

        public ResourceChunkSaveData WriteSaveData()
        {
            throw new NotImplementedException();
        }

        public void Regenerate()
        {

        }

        public void Unload()
        {
            foreach (var resource in resources)
            {
                if (resource.IsDirty)
                {
                    resource.WriteSaveData();
                    /// continue writing
                }
            }
#if UNITY_EDITOR
            DestroyImmediate(this.gameObject);
#else
            Destroy(this.gameObject);
#endif
        }
        
        /// <summary>
        /// Returns the texture index with the highest alpha value, given a point in world space.
        /// </summary>
        TerrainLayer GetTerrainLayerFromPoint(Terrain terrain, Vector3 point)
        {
            var terrainData = terrain.terrainData;
            // Convert world position to terrain local position
            int mapX = Mathf.RoundToInt((point.x - terrain.transform.position.x) / terrainData.size.x * terrainData.alphamapWidth);
            int mapZ = Mathf.RoundToInt((point.z - terrain.transform.position.z) / terrainData.size.z * terrainData.alphamapHeight);

            // Get the texture mix at the specified point
            float[,,] alphaMap = terrainData.GetAlphamaps(mapX, mapZ, 1, 1);

            // Find the dominant texture index
            int maxIndex = 0;
            float maxAlpha = 0f;
            for (int i = 0; i < alphaMap.GetLength(2); i++)
            {
                if (alphaMap[0, 0, i] > maxAlpha)
                {
                    maxAlpha = alphaMap[0, 0, i];
                    maxIndex = i;
                }
            }

            return terrain.terrainData.terrainLayers[maxIndex]; // Returns the index of the dominant texture
        }

#if UNITY_EDITOR 
        void OnDrawGizmosSelected()
        {
            if (gameObject.activeInHierarchy)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireCube(this.transform.position, Vector3.one * this.chunkSize);
            }
        }
#endif
    }
    
    [BurstCompile]
    public struct CalculateNoiseRandom_Job : IJobParallelFor
    {
        [ReadOnly] public int ChunkSize;
        [ReadOnly] public int Seed;
        [ReadOnly] public int Width;
        [ReadOnly] public int Height;
        [ReadOnly] public int3 Offset;
        [ReadOnly] public float Density;
        [ReadOnly] public float Scale;

        public NativeArray<float> NoiseMap;

        public void Execute(int index)
        {
            var rand = Unity.Mathematics.Random.CreateFromIndex((uint)index);
            rand.state = (uint)math.abs(Seed);
            var density01 = math.clamp(Density, 0, 1);
            
            int x = index % ChunkSize;
            int y = (index / ChunkSize) % ChunkSize;
            int z = index / (ChunkSize * ChunkSize);
            
            float sx = (x + Offset.x) * Scale;
            float sy = (y + Offset.y) * Scale;
            float sz = (z + Offset.z) * Scale;

            NoiseMap[index] = noise.snoise(new float3(sx, sy, sz)); /// 3D simplex
        }
    }

}