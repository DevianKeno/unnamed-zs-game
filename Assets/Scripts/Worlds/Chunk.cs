using System;
using System.Collections.Generic;
using System.Linq;

using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.AddressableAssets;

using MEC;

using UZSG.Objects;
using UZSG.Saves;

namespace UZSG.Worlds
{
    public class Chunk : MonoBehaviour, ISaveDataReadWrite<ChunkSaveData>
    {
        public const float MAX_TREE_DENSITY = 0.5f;
        const int JOB_BATCH_SIZE = 64;

        public int ChunkSize;
        public Vector3Int Coord;
        /// <summary>
        /// Takes the world position of the chunk, and offsets it by half towards 0.
        /// </summary>
        public Vector3 PositionOffset
        {
            get => new(
                transform.position.x - ChunkSize / 2,
                transform.position.y - ChunkSize / 2,
                transform.position.z - ChunkSize / 2);
        }
        public bool IsDirty { get; private set; }
        [SerializeField] int seed;
        [SerializeField] NoiseParameters treeNoiseParams;
        [SerializeField] NoiseParameters pickupsNoiseParams;
        [SerializeField] GenerateResourceChunkSettings settings;

        RaycastHit hit;
        /// <summary>
        /// List of resources populating this chunk.
        /// </summary>e
        [SerializeField] Dictionary<Vector3Int, Resource> population = new();

        /// <summary>
        /// The terrain this chunk is in.
        /// </summary>
        Terrain terrain;
        bool _isProcessing;
        JobHandle _currentJob;
        NativeList<int2> _treeCoords;
        CoroutineHandle _pollJobHandle;

        /// <summary>
        /// Populates t
        /// </summary>
        public void GenerateResources()
        {
            if (_isProcessing) return;

            _isProcessing = true;
            /// ^2 only, take into account only 2 dimensions since these are trees
            /// idk about the job tho
            int totalLength = ChunkSize * ChunkSize;
            int estimatedCapacity = Mathf.CeilToInt(totalLength * treeNoiseParams.Density * 1.5f); /// 50% extra capacity :( was 1%, then 5%, then 10% :(
            _treeCoords = new(estimatedCapacity, Allocator.TempJob);
            CalculateTreePlacementsJob calculateTreePlacementsJob = new()
            {
                ChunkSize = ChunkSize,
                Seed = treeNoiseParams.Seed,
                Offset = new(Coord.x, Coord.z),
                Density = Mathf.Clamp(treeNoiseParams.Density, 0, MAX_TREE_DENSITY),
                TreeCoords = _treeCoords.AsParallelWriter(),
            };
            _currentJob = calculateTreePlacementsJob.Schedule(totalLength, JOB_BATCH_SIZE);
            _pollJobHandle = Timing.RunCoroutine(_PollJobRoutine());
        }
        
        IEnumerator<float> _PollJobRoutine()
        {
            while (!_currentJob.IsCompleted)  // Keep waiting until the job is finished
            {
                yield return Timing.WaitForOneFrame; // Wait for the next frame before checking again
            }

            _currentJob.Complete();
            PopulateTrees(_treeCoords);
            _treeCoords.Dispose();
            // PopulatePickups(_pickupsCoords);
            // _pickupsCoords.Dispose();
            _isProcessing = false;
        }

        void PopulateTrees(NativeList<int2> treeCoords)
        {
            if (!settings.PlaceTrees) return;

            Vector3 samplePoint;
            int x, z;
            for (int i = 0; i < treeCoords.Length; i++)
            {
                x = treeCoords[i].x;
                z = treeCoords[i].y;
                samplePoint = PositionOffset + new Vector3(x, 0f, z);
                
                if (settings.PlaceTrees)
                {
                    TryPlaceTree(new(x, 0, z), samplePoint);
                    // TryPlaceTree(layer, samplePoint);
                }
            }
        }

        /// <summary>
        /// Tries to place a Tree (Resource) at position given the terrain layer.
        /// </summary>
        /// <param name="coord">The chunk-relative coordinate of this object</param>
        /// <param name="position">The position to place this object in world space</param>
        void TryPlaceTree(Vector3Int coord, Vector3 position)
        {
            if (population.ContainsKey(coord)) return;
            
            if (!Physics.Raycast(new(position.x, 256f, position.z), -Vector3.up, out hit, 500f)) return;
            if (!hit.collider.TryGetComponent(out terrain)) return;

            var layer = GetTerrainLayerFromPoint(terrain, hit.point, out float value);
            /// place trees only on grass layer
            if (layer.diffuseTexture.name.Equals("grass", StringComparison.OrdinalIgnoreCase) &&
                value >= 1f) /// only layers with full grass influence
            {
                population[coord] = null; /// mark/pre-populate, to prevent problems later
                Game.Objects.PlaceNew<Objects.Tree>("pine_tree_heart", hit.point, (info) =>
                {
                    var randomRotation = info.Object.Rotation;
                    randomRotation.y = 360f * Mathf.PerlinNoise(coord.x, coord.z);
                    info.Object.Rotation = randomRotation;
                    population[coord] = info.Object;
                });
            }
            else
            {
                /// do not place tree
            }
        }

        /// <summary>
        /// Tries to place an Ore Deposit at position given the terrain layer.
        /// </summary>
        /// <param name="coord">The chunk-relative coordinate of this object</param>
        /// <param name="position">The position to place this object in world space</param>
        void TryPlaceOreDeposit(Vector3Int coord, Vector3 position)
        {
            if (population.ContainsKey(coord)) return;
            
            if (!Physics.Raycast(new(position.x, 256f, position.z), -Vector3.up, out hit, 500f)) return;
            if (!hit.collider.TryGetComponent(out terrain)) return;

            var layer = GetTerrainLayerFromPoint(terrain, hit.point, out float value);
            if (layer.diffuseTexture.name.Equals("grass", StringComparison.OrdinalIgnoreCase) &&
                value >= 1f)
            {
                population[coord] = null; /// mark/pre-populate, to prevent problems later
                Game.Objects.PlaceNew<Objects.Tree>("iron_deposit", hit.point, (info) =>
                {
                    var randomRotation = info.Object.Rotation;
                    randomRotation.y = 360f * Mathf.PerlinNoise(coord.x, coord.z);
                    info.Object.Rotation = randomRotation;
                    population[coord] = info.Object;
                });
            }
            else
            {
                /// do not place
            }
        }

        void PopulatePickups(NativeList<int2> pickupsCoords)
        {
            if (!settings.PlacePickups) return;
            
            Vector3 samplePoint;
            int x, z;
            for (int i = 0; i < pickupsCoords.Length; i++)
            {
                x = pickupsCoords[i].x;
                z = pickupsCoords[i].y;
                samplePoint = PositionOffset + new Vector3(x, 0f, z);
                
                if (settings.PlacePickups)
                {
                    TryPlacePickup(new(x, 0, z), samplePoint);
                }
            }
        }

        /// <summary>
        /// Tries to place a Resource Pickup at position given the terrain layer.
        /// </summary>
        void TryPlacePickup(Vector3Int coord, Vector3 position)
        {
            if (population.ContainsKey(coord)) return;

        }

        public void SetGenerationSettings(GenerateResourceChunkSettings settings)
        {
            this.settings = settings;
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
        
        public void ReadSaveData(ChunkSaveData saveData)
        {
            foreach (var objectSave in saveData.Objects)
            {
                var position = Utils.FromFloatArray(objectSave.Transform.Position);
                Game.Objects.PlaceNew(objectSave.Id, position: position, callback: (info) =>
                {
                    info.Object.ReadSaveData(objectSave);
                });
            }
        }

        public ChunkSaveData WriteSaveData()
        {
            var csd = new ChunkSaveData
            {
                Coord = new int[3] { Coord.x, Coord.y, Coord.z }
            };
            var objects = new List<ObjectSaveData>();
            foreach (var resource in population.Values)
            {
                objects.Add(resource.WriteSaveData());
            } 
            csd.Objects = objects;
            return csd;
        }

        public void Regenerate()
        {

        }

        public void Unload()
        {
            foreach (var resource in population.Values)
            {
                if (resource.IsDirty)
                {
                    // resource.WriteSaveData();
                    // continue writing
                }
#if UNITY_EDITOR
                DestroyImmediate(resource.gameObject);
#else
                Destroy(resource.gameObject);
#endif
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
        TerrainLayer GetTerrainLayerFromPoint(Terrain terrain, Vector3 point, out float value)
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

            value = alphaMap[0, 0, maxIndex];
            return terrain.terrainData.terrainLayers[maxIndex]; // Returns the index of the dominant texture
        }

        void OnDrawGizmosSelected()
        {
            if (!gameObject.activeInHierarchy) return;
            
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(this.transform.position, Vector3.one * ChunkSize);
        }
    }
    
    /// <summary>
    /// Calculates the coordinates of trees in 2D space with random noise.
    /// </summary>
    [BurstCompile]
    public struct CalculateTreePlacementsJob : IJobParallelFor
    {
        [ReadOnly] public int ChunkSize;
        [ReadOnly] public int Seed;
        [ReadOnly] public int2 Offset;
        [ReadOnly] public float Density;
        /// <summary>
        /// The coordinates where resources are placed.
        /// </summary>
        public NativeList<int2>.ParallelWriter TreeCoords;

        public void Execute(int index)
        {
            var rand = Unity.Mathematics.Random.CreateFromIndex((uint)(Seed + index));
            var seedOffset = new float2(
                rand.NextFloat(Noise.MIN_RAND, Noise.MAX_RAND),
                rand.NextFloat(Noise.MIN_RAND, Noise.MAX_RAND));
            int x = index % ChunkSize;
            int y = (index / ChunkSize) % ChunkSize;

            var samplePos = new float2(x, y) + (seedOffset + Offset);
            uint hash = math.asuint(samplePos.x * 73856093 + samplePos.y * 19349663 + Seed);
            var cellRand = Unity.Mathematics.Random.CreateFromIndex(hash);
            float randomValue = cellRand.NextFloat(0f, 1f);
        
            if (randomValue < Density)
            {
                TreeCoords.AddNoResize(new int2(x, y));
            }
        }
    }
        
    [BurstCompile]
    public struct CalculateNoiseSimplex_Job : IJobParallelFor
    {
        // [ReadOnly] public int ChunkSize;
        // [ReadOnly] public int Seed;
        // [ReadOnly] public int3 Offset;
        // [ReadOnly] public float Density;

        // public NativeArray<float> NoiseMap;

        public void Execute(int index)
        {
        //     var rand = Unity.Mathematics.Random.CreateFromIndex(math.hash(new int2(Seed, index)));
        //     var seedOffset = new float3(
        //         rand.NextFloat(float.MinValue, float.MaxValue),
        //         rand.NextFloat(float.MinValue, float.MaxValue),
        //         rand.NextFloat(float.MinValue, float.MaxValue));

        //     int x = index % ChunkSize;
        //     int y = (index / ChunkSize) % ChunkSize;
        //     int z = index / (ChunkSize * ChunkSize);
        //     float scale = math.clamp(Scale, 0.0001f, Scale);
            
        //     float sx = (x + Offset.x + seedOffset.x) * scale;
        //     float sy = (y + Offset.y + seedOffset.y) * scale;
        //     float sz = (z + Offset.z + seedOffset.z) * scale;

        //     NoiseMap[index] = noise.snoise(new float3(sx, sy, sz));
        }
    }
}