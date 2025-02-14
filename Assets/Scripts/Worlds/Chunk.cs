using System;
using System.Collections.Generic;
using System.Linq;

using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.AddressableAssets;

using MEC;

using UZSG.Data;
using UZSG.Objects;
using UZSG.Saves;

namespace UZSG.Worlds
{
    public class Chunk : MonoBehaviour, ISaveDataReadWrite<ChunkSaveData>
    {
        public const float DEFAULT_TREE_DENSITY = 0.0066f;
        public const float MAX_TREE_DENSITY = 0.5f;
        public const float DEFAULT_PICKUPS_DENSITY = 0.03f;
        public const float MAX_PICKUPS_DENSITY = 0.5f;
        public const float DEFAULT_ORE_DEPOSITS_DENSITY = 0.0001f;
        public const float MAX_ORE_DEPOSITS_DENSITY = 0.001f;
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
        /// <summary>
        /// Whether to save this chunk on world exit.
        /// </summary>
        public bool IsDirty { get; private set; } = false;
        [SerializeField] int seed;
        [SerializeField] internal NoiseData treesNoiseData;
        [SerializeField] internal NoiseData pickupsNoiseData;
        [SerializeField] internal NoiseData oreDepositsNoiseData;
        
        [SerializeField] GenerateResourceChunkSettings settings;
        /// <summary>
        /// List of objects placed in this chunk.
        /// </summary>
        List<BaseObject> _objects = new();
        /// <summary>
        /// List of resources populating this chunk.
        /// </summary>
        Dictionary<Vector3Int, BaseObject> _population = new();
        RaycastHit hit;
        /// <summary>
        /// The terrain this chunk is in.
        /// </summary>
        Terrain terrain;
        bool _isProcessing;
        JobHandle _treesPlacementsJob;
        JobHandle _pickupsPlacementsJob;
        JobHandle _oreDepositsPlacementsJob;
        NativeList<int2> _treeCoords;
        NativeList<int2> _pickupsCoords;
        NativeList<int2> _oreDepositsCoords;

        /// <summary>
        /// Populates the chunk with resources.
        /// </summary>
        internal void GenerateResources()
        {
            if (_isProcessing) return;

            _isProcessing = true;
            GenerateTrees();
            GeneratePickups();
            GenerateOreDeposits();
        }


        #region Generation

        void GenerateTrees()
        {
            /// ^2 only, take into account only 2 dimensions since these are trees
            /// idk about the job tho
            int totalLength = ChunkSize * ChunkSize;
            int estimatedCapacity = Mathf.CeilToInt(totalLength * treesNoiseData.Noise.Density * 1.5f); /// 50% extra capacity :( was 1%, then 5%, then 10% :(
            _treeCoords = new(estimatedCapacity, Allocator.TempJob);
            CalculateTreePlacementsJob job = new()
            {
                ChunkSize = ChunkSize,
                Seed = this.seed,
                Offset = new(Coord.x * ChunkSize, Coord.z * ChunkSize),
                Density = Mathf.Clamp(treesNoiseData.Noise.Density, 0, MAX_TREE_DENSITY),
                TreeCoords = _treeCoords.AsParallelWriter(),
            };
            _treesPlacementsJob = job.Schedule(totalLength, JOB_BATCH_SIZE);
            Timing.RunCoroutine(_PollTreePlacementsJobRoutine());
        }

        void GeneratePickups()
        {
            /// ^2 only, take into account only 2 dimensions
            int totalLength = ChunkSize * ChunkSize;
            int estimatedCapacity = Mathf.CeilToInt(totalLength * pickupsNoiseData.Noise.Density * 1.5f); /// 50% extra capacity :( was 1%, then 5%, then 10% :(
            _pickupsCoords = new(estimatedCapacity, Allocator.TempJob);
            CalculatePickupsPlacementsJob job = new()
            {
                ChunkSize = ChunkSize,
                Seed = this.seed,
                Offset = new(Coord.x * ChunkSize, Coord.z * ChunkSize),
                Density = Mathf.Clamp(pickupsNoiseData.Noise.Density, 0, MAX_PICKUPS_DENSITY),
                PickupsCoords = _pickupsCoords.AsParallelWriter(),
            };
            _pickupsPlacementsJob = job.Schedule(totalLength, JOB_BATCH_SIZE);
            Timing.RunCoroutine(_PollPickupsPlacementsJobRoutine());
        }

        void GenerateOreDeposits()
        {
            /// ^2 only, take into account only 2 dimensions
            int totalLength = ChunkSize * ChunkSize;
            int estimatedCapacity = Mathf.CeilToInt(totalLength * oreDepositsNoiseData.Noise.Density * 1.5f); /// 50% extra capacity :( was 1%, then 5%, then 10% :(
            _oreDepositsCoords = new(estimatedCapacity, Allocator.TempJob);
            CalculateOreDepositsPlacementsJob job = new()
            {
                ChunkSize = ChunkSize,
                Seed = this.seed,
                Offset = new(Coord.x * ChunkSize, Coord.z * ChunkSize),
                Density = Mathf.Clamp(oreDepositsNoiseData.Noise.Density, 0, MAX_ORE_DEPOSITS_DENSITY),
                OreDepositsCoords = _oreDepositsCoords.AsParallelWriter(),
            };
            _oreDepositsPlacementsJob = job.Schedule(totalLength, JOB_BATCH_SIZE);
            Timing.RunCoroutine(_PollOreDepositsPlacementsJobRoutine());
        }

        #endregion


        IEnumerator<float> _PollTreePlacementsJobRoutine()
        {
            while (!_treesPlacementsJob.IsCompleted)
            {
                yield return Timing.WaitForOneFrame;
            }

            _treesPlacementsJob.Complete();
            PopulateTrees(_treeCoords);
            _treeCoords.Dispose();
        }

        IEnumerator<float> _PollPickupsPlacementsJobRoutine()
        {
            while (!_pickupsPlacementsJob.IsCompleted)
            {
                yield return Timing.WaitForOneFrame;
            }

            _pickupsPlacementsJob.Complete();
            PopulatePickups(_pickupsCoords);
            _pickupsCoords.Dispose();
        }

        IEnumerator<float> _PollOreDepositsPlacementsJobRoutine()
        {
            while (!_oreDepositsPlacementsJob.IsCompleted)
            {
                yield return Timing.WaitForOneFrame;
            }

            _oreDepositsPlacementsJob.Complete();
            PopulateOreDeposits(_oreDepositsCoords);
            _oreDepositsCoords.Dispose();
        }


        #region Trees placements

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
                
                TryPlaceTree(new(x, 0, z), samplePoint);
            }
        }

        /// <summary>
        /// Tries to place a Tree (Resource) at position given the terrain layer.
        /// </summary>
        /// <param name="coord">The chunk-relative coordinate of this object</param>
        /// <param name="position">The position to place this object in world space</param>
        void TryPlaceTree(Vector3Int coord, Vector3 position)
        {
            if (_population.ContainsKey(coord)) return;
            
            if (!Physics.Raycast(new(position.x, 256f, position.z), -Vector3.up, out hit, 500f)) return;
            if (!hit.collider.TryGetComponent(out terrain)) return;

            var layer = GetTerrainLayerFromPoint(terrain, hit.point, out float value);
            /// place trees only on grass layer
            if (layer.diffuseTexture.name.Equals("grass", StringComparison.OrdinalIgnoreCase) &&
                value >= 1f) /// only layers with full grass influence
            {
                _population[coord] = null; /// mark/pre-populate, to prevent problems later
                Game.Objects.PlaceNew<Objects.TreeResource>("pine_tree_heart", hit.point, (info) =>
                {
                    var randomRotation = info.Object.Rotation.eulerAngles;
                    randomRotation.y = GetRandomRotationPerlin(coord.x, coord.z);
                    info.Object.Rotation = Quaternion.Euler(randomRotation);
                    _population[coord] = info.Object;
                });
            }
            else
            {
                /// do not place tree
            }
        }

        #endregion


        #region Pickups placements

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
                
                TryPlacePickup(new(x, 0, z), samplePoint);
            }
        }

        /// <summary>
        /// Tries to place a Resource Pickup at position given the terrain layer.
        /// </summary>
        void TryPlacePickup(Vector3Int coord, Vector3 position)
        {
            if (_population.ContainsKey(coord)) return;
            
            if (!Physics.Raycast(new(position.x, 256f, position.z), -Vector3.up, out hit, 500f)) return;
            if (!hit.collider.TryGetComponent(out terrain)) return;

            var layer = GetTerrainLayerFromPoint(terrain, hit.point, out float value);
            if (layer.diffuseTexture.name.Equals("grass", StringComparison.OrdinalIgnoreCase) &&
                value >= 1f)
            {
                _population[coord] = null; /// mark/pre-populate, to prevent problems later
                Game.Objects.PlaceNew<ResourcePickup>("wild_grass", hit.point, (info) =>
                {
                    info.Object.transform.SetParent(transform);
                    var randomRotation = info.Object.Rotation.eulerAngles;
                    randomRotation.y = GetRandomRotationPerlin(coord.x, coord.z);
                    info.Object.Rotation = Quaternion.Euler(randomRotation);
                    _population[coord] = info.Object;
                });
            }
            else
            {
                /// do not place
            }
        }

        #endregion


        #region Ore deposit placements

        void PopulateOreDeposits(NativeList<int2> oreDepositsCoords)
        {
            if (!settings.PlaceOreDeposits) return;
            
            Vector3 samplePoint;
            int x, z;
            for (int i = 0; i < oreDepositsCoords.Length; i++)
            {
                x = oreDepositsCoords[i].x;
                z = oreDepositsCoords[i].y;
                samplePoint = PositionOffset + new Vector3(x, 0f, z);
                
                TryPlaceOreDeposit(new(x, 0, z), samplePoint);
            }
        }
        
        /// <summary>
        /// Tries to place an Ore Deposit at position given the terrain layer.
        /// </summary>
        /// <param name="coord">The chunk-relative coordinate of this object</param>
        /// <param name="position">The position to place this object in world space</param>
        void TryPlaceOreDeposit(Vector3Int coord, Vector3 position)
        {
            if (_population.ContainsKey(coord)) return;
            
            if (!Physics.Raycast(new(position.x, 256f, position.z), -Vector3.up, out hit, 500f)) return;
            if (!hit.collider.TryGetComponent(out terrain)) return;

            var layer = GetTerrainLayerFromPoint(terrain, hit.point, out float value);
            if (layer.diffuseTexture.name.Equals("grass", StringComparison.OrdinalIgnoreCase) &&
                value >= 1f)
            {
                _population[coord] = null; /// mark/pre-populate, to prevent problems later
                Game.Objects.PlaceNew<OreDeposit>("iron_deposit", hit.point, (info) =>
                {
                    info.Object.transform.SetParent(transform);
                    var randomRotation = info.Object.Rotation.eulerAngles;
                    randomRotation.y = GetRandomRotationPerlin(coord.x, coord.z);
                    info.Object.Rotation = Quaternion.Euler(randomRotation);
                    _population[coord] = info.Object;
                });
            }
            else
            {
                /// do not place
            }
        }

        #endregion


        float GetRandomRotationPerlin(float x, float z)
        {
            return 360f * Mathf.PerlinNoise(
                x + (this.Coord.x * ChunkSize),
                z + (this.Coord.y * ChunkSize));
        }


        #region Event callbacks

        void OnObjectDestructed(BaseObject baseObj)
        {
            if (_objects.Contains(baseObj))
            {
                _objects.Remove(baseObj);
            }
        }

        #endregion

        
        public void ReadSaveData(ChunkSaveData saveData)
        {
            foreach (var objectSave in saveData.Objects)
            {
                try
                {
                    var position = Utils.FromFloatArray(objectSave.Transform.Position);
                    Game.Objects.PlaceNew(objectSave.Id, position: position, callback: (info) =>
                    {
                        Game.Saves.ReadAsBaseObject(info.Object, objectSave);
                    });
                }
                catch (Exception ex)
                {
                    Game.Console.LogError($"An internal error occured when reading object save '{objectSave.Id}'", true);
                    Debug.LogException(ex);
                }
            }
        }

        public ChunkSaveData WriteSaveData()
        {
            var chunkSaveData = new ChunkSaveData
            {
                Coord = new int[3]{ Coord.x, Coord.y, Coord.z }
            };
            
            chunkSaveData.Objects.Clear();
            foreach (var obj in _objects)
            {
                if (obj == null) continue;

                chunkSaveData.Objects.Add(Game.Saves.WriteAsBaseObject(obj));
            }
            foreach (var resObj in _population.Values)
            {
                if (resObj == null ||
                    resObj.IsDirty == false ||
                    resObj is not Resource resource)
                {
                    continue;
                }
                chunkSaveData.Objects.Add(Game.Saves.WriteAsBaseObject(resource));
            }

            return chunkSaveData;
        }

        internal void RegisterObject(BaseObject baseObject)
        {
            _objects.Add(baseObject);
            baseObject.OnDestructed += OnObjectDestructed;
            MarkDirty();
        }

        internal void Regenerate()
        {

        }

        internal void Unload()
        {
            foreach (var resource in _population.Values)
            {
                if (resource.IsDirty)
                {
                    // resource.WriteSaveData();
                    // continue writing
                }
            }
#if UNITY_EDITOR
            DestroyImmediate(this.gameObject);
#else
            Destroy(this.gameObject);
#endif
        }
        
        
        #region Public

        public void SetGenerationSettings(GenerateResourceChunkSettings settings)
        {
            this.settings = settings;
        }

        public void SetSeed(int seed)
        {
            this.seed = seed;
            UnityEngine.Random.InitState(this.seed);
        }

        public void MarkDirty()
        {
            IsDirty = true;
        }

        #endregion


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
    
    /// <summary>
    /// Calculates the coordinates of trees in 2D space with random noise.
    /// </summary>
    [BurstCompile]
    public struct CalculatePickupsPlacementsJob : IJobParallelFor
    {
        [ReadOnly] public int ChunkSize;
        [ReadOnly] public int Seed;
        [ReadOnly] public int2 Offset;
        [ReadOnly] public float Density;
        /// <summary>
        /// The coordinates where resources are placed.
        /// </summary>
        public NativeList<int2>.ParallelWriter PickupsCoords;

        public void Execute(int index)
        {
            var rand = Unity.Mathematics.Random.CreateFromIndex((uint)(Seed + index));
            var seedOffset = new float2(
                rand.NextFloat(Noise.MIN_RAND, Noise.MAX_RAND),
                rand.NextFloat(Noise.MIN_RAND, Noise.MAX_RAND));
            int x = index % ChunkSize;
            int y = (index / ChunkSize) % ChunkSize;

            var samplePos = new float2(x, y) + (seedOffset + Offset);
            uint hash = math.asuint(samplePos.x * 95214531 + samplePos.y * 45889621 + Seed);
            var cellRand = Unity.Mathematics.Random.CreateFromIndex(hash);
            float randomValue = cellRand.NextFloat(0f, 1f);
        
            if (randomValue < Density)
            {
                PickupsCoords.AddNoResize(new int2(x, y));
            }
        }
    }

    /// <summary>
    /// Calculates the coordinates of trees in 2D space with random noise.
    /// </summary>
    [BurstCompile]
    public struct CalculateOreDepositsPlacementsJob : IJobParallelFor
    {
        [ReadOnly] public int ChunkSize;
        [ReadOnly] public int Seed;
        [ReadOnly] public int2 Offset;
        [ReadOnly] public float Density;
        /// <summary>
        /// The coordinates where resources are placed.
        /// </summary>
        public NativeList<int2>.ParallelWriter OreDepositsCoords;

        public void Execute(int index)
        {
            var rand = Unity.Mathematics.Random.CreateFromIndex((uint)(Seed + index));
            var seedOffset = new float2(
                rand.NextFloat(Noise.MIN_RAND, Noise.MAX_RAND),
                rand.NextFloat(Noise.MIN_RAND, Noise.MAX_RAND));
            int x = index % ChunkSize;
            int y = (index / ChunkSize) % ChunkSize;

            var samplePos = new float2(x, y) + (seedOffset + Offset);
            uint hash = math.asuint(samplePos.x * 65874491 + samplePos.y * 32254498 + Seed);
            var cellRand = Unity.Mathematics.Random.CreateFromIndex(hash);
            float randomValue = cellRand.NextFloat(0f, 1f);
        
            if (randomValue < Density)
            {
                OreDepositsCoords.AddNoResize(new int2(x, y));
            }
        }
    }
}