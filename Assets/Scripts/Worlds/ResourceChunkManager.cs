#define DEBUGGING_ENABLED

using System;
using System.Collections.Generic;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

using MEC;

using UZSG.Entities;

namespace UZSG.Worlds
{
    /// <summary>
    /// Dynamically loads resource objects in the world.
    /// </summary>
    [ExecuteInEditMode]
    public class ResourceChunkManager : MonoBehaviour
    {
        public const int MIN_CHUNK_RENDER_DISTANCE = 2;
        public const int MAX_CHUNK_RENDER_DISTANCE = 32;
        const int CHUNK_JOB_BATCH_SIZE = 4;

        public World World { get; set; }
        Player player;

        [Range(MIN_CHUNK_RENDER_DISTANCE, MAX_CHUNK_RENDER_DISTANCE), SerializeField] int renderDistance = 6;
        public int RenderDistance
        {
            get => renderDistance;
            set
            {
                var dist = value % 2 == 0 ? value : value - 1; /// floor even number
                renderDistance = Math.Clamp(dist, MIN_CHUNK_RENDER_DISTANCE, MAX_CHUNK_RENDER_DISTANCE);
            }
        } // set in Inspector
        
        public bool EnableLoadingChunks = false;
        [SerializeField] ChunkSizes chunkSize = ChunkSizes.Full;
        int chunkSizeInt => (int) chunkSize;
        [SerializeField] Terrain terrain;

        [Header("Generation Settings")]
        [Header("Trees")]
        public int Seed = 12345;
        [SerializeField] NoiseParameters treeNoiseParameters = new();
        public bool PlaceTrees = true;
        [Range(0, 1)] public float TreeDensity = 0.5f;
        [Header("Pickups")]
        [SerializeField] NoiseParameters pickupNoiseParameters = new();
        public bool PlacePickups = true;
        [Range(0, 1)] public float PickupsDensity = 0.5f;
        [SerializeField] int chunksLoaded;

        bool _isProcessingChunks;
        /// <summary>
        /// The chunk coordinate the player is currently in.
        /// </summary>
        Vector3Int playerChunkCoord;
        /// <summary>
        /// The last record chunk coordinate of the player.
        /// </summary>
        Vector3Int lastPlayerChunkCoord;
        Vector3Int camChunkCoord; /// [EditorOnly]
        Vector3Int lastCamChunkCoord; /// [EditorOnly]
        /// <summary>
        /// The currently loaded chunks <b> around the player.
        /// <c>Vector3Int</c> is the chunk coordinate.
        /// </summary>
        Dictionary<Vector3Int, ResourceChunk> currentChunks = new();
        /// <summary>
        /// Loaded chunks in memory from the world save.
        /// <c>Vector3Int</c> is the chunk coordinate.
        /// </summary>
        Dictionary<Vector3Int, ResourceChunk> loadedChunks = new();

        bool _jobInProgress = false;
        JobHandle _currentJob;
        NativeList<Vector3Int> _chunkCoords;
        NativeList<Vector3Int> _chunksToLoad;
        NativeList<Vector3Int> _chunksToUnload;

        [SerializeField] Transform chunksContainer;

        [Header("Debugging")]
        [Range(0, 1f)] public float chunkOpacity = 0.5f;
        [SerializeField] bool enableLODChunking = false;
        Color chunkColorLOD0 = new(0f, 1f, 0f); // Green (LOD 0)
        Color chunkColorLOD1 = new(1f, 1f, 0f); // Yellow (LOD 1)
        Color chunkColorLOD2 = new(1f, 0f, 0f); // Red (LOD 2)

        void Awake()
        {
            World = GetComponentInParent<World>();
        }

        internal void Initialize()
        {   
            EnableLoadingChunks = false;
            if (World == null)
            {
                Game.Console.LogFatal($"[World::ResourceChunkManager]: Unable to find world!");
                return;
            }
            
            Game.Entity.OnEntitySpawned += OnEntitySpawned;
            currentChunks.Clear();

            _chunkCoords = new(Allocator.Persistent);
            _chunksToLoad = new(Allocator.Persistent);
            _chunksToUnload = new(Allocator.Persistent);
        }


        #region Event callbacks

        void OnDestroy()
        {
            Game.Entity.OnEntitySpawned -= OnEntitySpawned;

            if (_chunkCoords.IsCreated) _chunkCoords.Dispose();
            if (_chunksToLoad.IsCreated) _chunksToLoad.Dispose();
            if (_chunksToUnload.IsCreated) _chunksToUnload.Dispose();
        }

        void OnEntitySpawned(EntityManager.EntityInfo info)
        {
            if (info.Entity is not Player player) return;

            Game.Entity.OnEntitySpawned -= OnEntitySpawned;
            this.player = player;
            if (this.player == null)
            {
                Game.Console.LogError($"[World::ResourceManager]: No local player found!");
                return;
            }
            EnableLoadingChunks = true;
        }

        void OnValidate()
        {
            treeNoiseParameters.Seed = this.Seed;
            pickupNoiseParameters.Seed = this.Seed;
        }

        #endregion


        void FixedUpdate()
        {
            if (!EnableLoadingChunks) return;

            if (Application.isPlaying || _isProcessingChunks)
            {
                playerChunkCoord = ToChunkCoord(player.Position);

                /// Regenerate chunks if the player moves to a new chunk
                if (playerChunkCoord != lastPlayerChunkCoord)
                {
                    StartResourceChunksProcessing();
                    lastPlayerChunkCoord = playerChunkCoord;
                }
            }
            else
            {
#if UNITY_EDITOR
                if (SceneView.lastActiveSceneView == null || _isProcessingChunks) return;
            
                Vector3 camPos = SceneView.lastActiveSceneView.camera.transform.position;
                camChunkCoord = ToChunkCoord(camPos);

                if (camChunkCoord != lastCamChunkCoord)
                {
                    StartResourceChunksProcessing();
                    lastCamChunkCoord = camChunkCoord;
                }
#endif
            }
            
            if (_jobInProgress && _currentJob.IsCompleted)
            {
                _currentJob.Complete();
                if (_chunkCoords.IsCreated) _chunkCoords.Dispose();
                FinishResourceChunksProcessing();
            }
        }
        
        void StartResourceChunksProcessing()
        {
            if (_jobInProgress) return;

            _jobInProgress = true;
            _isProcessingChunks = true;
            Vector3Int currentCoord = playerChunkCoord;
#if UNITY_EDITOR
            if (!Application.isPlaying) currentCoord = camChunkCoord;
#endif

            // int i = 0;
            // foreach (var key in currentChunks.Keys)
            // {
            //     _chunkCoords[i++] = key;
            // }

            /// Collect currently loaded chunk coordinates
            for (int z = -renderDistance; z < renderDistance; z++)
            {
                for (int x = -renderDistance; x < renderDistance; x++)
                {
                    Vector3Int coord = new(
                        currentCoord.x + x,
                        0,
                        currentCoord.z + z);
                    
                    _chunksToLoad.Add(coord); /// already loaded chunks are handled later on
                }
            }

            /// Create job
            ChunkGenerationJob chunkJob = new()
            {
                RenderDistance = renderDistance,
                PlayerChunkCoord = playerChunkCoord,
                ChunkCoords = _chunkCoords.AsArray(),
                ChunksToLoad = _chunksToLoad.AsParallelWriter(),
                ChunksToUnload = _chunksToUnload.AsParallelWriter()
            };

            /// Schedule job
            _currentJob = chunkJob.Schedule(_chunkCoords.Length, CHUNK_JOB_BATCH_SIZE);
        }

        void FinishResourceChunksProcessing()
        {
            _jobInProgress = false;
            _isProcessingChunks = false;

            // Process loaded/unloaded chunks
            Timing.RunCoroutine(_ProcessChunks());
        }

        IEnumerator<float> _ProcessChunks()
        {
            yield return Timing.WaitUntilDone(Timing.RunCoroutine(_LoadChunksRoutine(_chunksToLoad)));
            yield return Timing.WaitUntilDone(Timing.RunCoroutine(_UnloadChunksRoutine(_chunksToUnload)));

            // if (_chunksToLoad.IsCreated) _chunksToLoad.Dispose();
            // if (_chunksToUnload.IsCreated) _chunksToUnload.Dispose();
        }

        IEnumerator<float> _LoadChunksRoutine(NativeList<Vector3Int> chunksToLoad)
        {
            foreach (var coord in chunksToLoad)
            {
                if (loadedChunks.ContainsKey(coord)) continue;
                
                var chunk = CreateChunkNew(coord);
                currentChunks[coord] = chunk ;
                loadedChunks[coord] = chunk;

                yield return 0f;
            }
            if (_chunksToLoad.IsCreated) _chunksToLoad.Dispose();
        }

        IEnumerator<float> _UnloadChunksRoutine(NativeList<Vector3Int> chunksToUnload)
        {
            foreach (var coord in chunksToUnload)
            {
                if (!currentChunks.TryGetValue(coord, out ResourceChunk chunk)) continue;

                chunk.Unload();
                currentChunks.Remove(coord);

                var msg = $"Unloaded resource chunk ({chunk.Coord.x}, {chunk.Coord.z})";
                print(msg);
                yield return 0f;
            }
            if (_chunksToUnload.IsCreated) _chunksToUnload.Dispose();
        }

        /// <summary>
        /// Checks whether a chunk is currently loaded given a coordinate.
        /// </summary>
        bool ChunkExistsAt(Vector3Int coord)
        {
            return currentChunks.ContainsKey(coord);
        }

        /// <summary>
        /// Takes a world position and converts it to the chunk coordinate it's in.
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        Vector3Int ToChunkCoord(Vector3 position)
        {
            return new Vector3Int(
                Mathf.FloorToInt(position.x / chunkSizeInt),
                Mathf.FloorToInt(position.y / chunkSizeInt),
                Mathf.FloorToInt(position.z / chunkSizeInt)
            );
        }

        ResourceChunk CreateChunkNew(Vector3Int coord)
        {
            var chunk = new GameObject($"Resource Chunk ({coord.x}, {coord.y}, {coord.z})", typeof(ResourceChunk)).GetComponent<ResourceChunk>();
            chunk.transform.SetParent(chunksContainer);
            chunk.transform.position = new Vector3(
                coord.x * (int) chunkSize + chunkSizeInt,
                coord.y,
                coord.z * (int) chunkSize + chunkSizeInt
            );
            chunk.Coord = coord;
            chunk.SetTreeNoiseParameters(treeNoiseParameters);
            chunk.SetPickupNoiseParameters(pickupNoiseParameters);

#if UNITY_EDITOR
            chunk.SetSeed(this.Seed);
#else
            chunk.SetSeed(World.GetSeed());
#endif

            var generationSettings = new GenerateResourceChunkSettings()
            {
                PlaceTrees = PlaceTrees,
                TreeDensity = TreeDensity,
                PlacePickups = PlacePickups,
                PickupsDensity = PickupsDensity,
            };
            chunk.GenerateResources((int) chunkSize, coord, generationSettings);
            
            var msg = $"Created new resource chunk at ({coord.x}, {coord.y}, {coord.z})";
            print(msg);
            
            return chunk;
        }
        
        // ResourceChunk LoadChunkExisting(Vector3Int coord, ResourceChunkSaveData saveData)
        // {
        //     var chunk = new GameObject($"Resource Chunk ({coord.x}, {coord.y}, {coord.z})", typeof(ResourceChunk)).GetComponent<ResourceChunk>();
        //     chunk.transform.SetParent(chunksContainer);
        //     chunk.transform.position = new Vector3(
        //         coord.x * (int) chunkSize + chunkSizeInt,
        //         coord.y,
        //         coord.z * (int) chunkSize + chunkSizeInt
        //     );
        //     chunk.Coord = coord;
        //     chunk.ReadSaveData(saveData);
        //     print($"Loaded existing resource chunk at ({coord.x}, {coord.y}, {coord.z})");

        //     return chunk;
        // }

        /// <summary>
        /// Prints only when the header definition 'DEBUGGING_ENABLED' is defined
        /// </summary>
        /// <param name="message"></param>
        [System.Diagnostics.Conditional("DEBUGGING_ENABLED")]
        new void print(object message)
        {
            Debug.Log(message);
        }


#region Editor gizmo
#if UNITY_EDITOR
        void OnDrawGizmos()
        {
            if (!gameObject.activeInHierarchy) return;
            if (Application.isPlaying || SceneView.lastActiveSceneView == null) return;
            
            Vector3Int camChunkCoord = ToChunkCoord(SceneView.lastActiveSceneView.camera.transform.position);
            int maxRenderDistance;
            if (enableLODChunking) /// buggy
            {
                maxRenderDistance = renderDistance * 2; // Extend LOD visibility range
            }
            else
            {
                maxRenderDistance = renderDistance;
            }

            for (int x = -maxRenderDistance; x <= maxRenderDistance; x++)
            {
                for (int z = -maxRenderDistance; z <= maxRenderDistance; z++)
                {
                    if (enableLODChunking) /// buggy
                    {
                        int lodSize = GetLODSize(x, z, chunkSizeInt);
                        Vector3Int chunkCoord = new(camChunkCoord.x + x, camChunkCoord.z + z);
                        Vector3Int alignedCoord = AlignToLODGrid(chunkCoord, lodSize);
                        Vector3 chunkPos = new(
                            alignedCoord.x * lodSize + (lodSize / 2f),
                            0,
                            alignedCoord.z * lodSize + (lodSize / 2f)
                        );
                        DrawLODChunk(chunkPos, lodSize);
                    }
                    else
                    {
                        Vector3 chunkPos = new(
                            (camChunkCoord.x + x) * chunkSizeInt + (chunkSizeInt / 2), 
                            0, 
                            (camChunkCoord.z + z) * chunkSizeInt + (chunkSizeInt / 2)
                        );

                        DrawChunkGrid(chunkPos, chunkSizeInt);
                    }
                }
            }
        }

        void DrawChunkGrid(Vector3 chunkPos, int size)
        {
            Vector3 topLeft = chunkPos;
            Vector3 topRight = chunkPos + new Vector3(size, 0, 0);
            Vector3 bottomLeft = chunkPos + new Vector3(0, 0, size);
            Vector3 bottomRight = chunkPos + new Vector3(size, 0, size);

            Handles.DrawLine(topLeft, topRight);
            Handles.DrawLine(topRight, bottomRight);
            Handles.DrawLine(bottomRight, bottomLeft);
            Handles.DrawLine(bottomLeft, topLeft);
        }

        /// <summary>
        /// Determines LOD size based on Manhattan distance.
        /// </summary>
        int GetLODSize(int dx, int dz, int baseSize)
        {
            int distance = Mathf.Max(Mathf.Abs(dx), Mathf.Abs(dz)); // Square-based distance

            if (distance <= renderDistance) return baseSize;       // LOD 0 (Default size)
            if (distance <= renderDistance * 1.5f) return baseSize * 2; // LOD 1 (2x Larger)
            return baseSize * 4;                                   // LOD 2 (4x Larger)
        }

        /// <summary>
        /// Draws a chunk with LOD-based color.
        /// </summary>
        void DrawLODChunk(Vector3 chunkPos, int size)
        {
            Vector3 topLeft = chunkPos + new Vector3(-size / 2, 0, -size / 2);
            Vector3 topRight = chunkPos + new Vector3(size / 2, 0, -size / 2);
            Vector3 bottomLeft = chunkPos + new Vector3(-size / 2, 0, size / 2);
            Vector3 bottomRight = chunkPos + new Vector3(size / 2, 0, size / 2);

            Color lodColor = GetLODColorFromSize(size);
            Handles.DrawSolidRectangleWithOutline(new Vector3[] { topLeft, topRight, bottomRight, bottomLeft }, lodColor * chunkOpacity, lodColor);
        }

        /// <summary>
        /// Ensures chunks align properly when using LOD sizes.
        /// </summary>
        Vector3Int AlignToLODGrid(Vector3Int chunkCoord, int lodSize)
        {
            int lodChunks = lodSize / (int)chunkSize; // Number of chunks per LOD level
            return new Vector3Int(
                Mathf.FloorToInt((float) chunkCoord.x / lodChunks),
                Mathf.FloorToInt((float) chunkCoord.y / lodChunks),
                Mathf.FloorToInt((float) chunkCoord.z / lodChunks)
            );
        }

        /// <summary>
        /// Determines LOD color based on distance.
        /// </summary>
        Color GetLODColorFromSize(int size)
        {
            if (size <= (int) chunkSize) return chunkColorLOD0;
            if (size <= (int) chunkSize * 2) return  chunkColorLOD1;
            return chunkColorLOD2;
        }

        public void RefreshChunks()
        {
            currentChunks.Clear();
        }
#endif
#endregion
    }

    [BurstCompile]
    public struct ChunkGenerationJob : IJobParallelFor
    {
        [ReadOnly] public int RenderDistance;
        /// <summary>
        /// The chunk coordinate the player is currently in.
        /// </summary>
        [ReadOnly] public Vector3Int PlayerChunkCoord;
        /// <summary>
        /// Currently loaded chunk coordinates.
        /// </summary>
        [ReadOnly] public NativeArray<Vector3Int> ChunkCoords;
        [ReadOnly] public NoiseParameters NoiseParameters;

        public NativeList<Vector3Int>.ParallelWriter ChunksToLoad;
        public NativeList<Vector3Int>.ParallelWriter ChunksToUnload;

        public void Execute(int index)
        {
            Vector3Int chunkCoord = ChunkCoords[index];
            bool shouldUnload = math.abs(chunkCoord.x - PlayerChunkCoord.x) > RenderDistance ||
                                // math.abs(chunkCoord.y - PlayerChunkCoord.y) > RenderDistance ||
                                math.abs(chunkCoord.z - PlayerChunkCoord.z) > RenderDistance;

            if (shouldUnload)
            {
                ChunksToUnload.AddNoResize(chunkCoord);
            }
            else
            {
                /// calculate
                // var noiseMap = Noise.generate2DRandom01(
                //     NoiseParameters
                // );
                ChunksToLoad.AddNoResize(chunkCoord);
            }
        }
    }
}