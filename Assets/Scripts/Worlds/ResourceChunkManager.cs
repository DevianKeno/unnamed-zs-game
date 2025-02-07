#define DEBUGGING_ENABLED

using System;
using System.Collections.Generic;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

using UZSG.Entities;
using UZSG.Saves;

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
        /// Currently loaded chunks.
        /// <c>Vector3Int</c> is the chunk coordinate.
        /// </summary>
        Dictionary<Vector3Int, ResourceChunk> currentChunks = new();
        /// <summary>
        /// Chunks that are saved in memory.
        /// </summary>
        Dictionary<Vector3Int, ResourceChunkSaveData> loadedChunks = new();

        [Header("Debugging")]
        [Range(0, 1f)] public float chunkOpacity = 0.5f;
        [SerializeField] bool enableLODChunking = false;
        Color chunkColorLOD0 = new(0f, 1f, 0f); /// Green (LOD 0)
        Color chunkColorLOD1 = new(1f, 1f, 0f); /// Yellow (LOD 1)
        Color chunkColorLOD2 = new(1f, 0f, 0f); /// Red (LOD 2)

        void Awake()
        {
            World = GetComponentInParent<World>();
        }

        internal void Initialize()
        {   
            if (World == null)
            {
                Game.Console.LogError($"[World::ResourceManager]: Must be initialized given a world!");
                return;
            }
            
            Game.Entity.OnEntitySpawned += OnEntitySpawned;
            loadedChunks.Clear();
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

        void FixedUpdate()
        {
            if (!EnableLoadingChunks) return;

            if (Application.isPlaying)
            {
                playerChunkCoord = ToChunkCoord(player.Position);

                // Regenerate chunks if the player moves to a new chunk
                if (playerChunkCoord != lastPlayerChunkCoord)
                {
                    GenerateResourceChunks();
                    lastPlayerChunkCoord = playerChunkCoord;
                }
            }
            else
            {
#if UNITY_EDITOR
                if (SceneView.lastActiveSceneView == null) return;
            
                Vector3 camPos = SceneView.lastActiveSceneView.camera.transform.position;
                camChunkCoord = ToChunkCoord(camPos);

                if (camChunkCoord != lastCamChunkCoord)
                {
                    GenerateResourceChunks();
                    lastCamChunkCoord = camChunkCoord;
                }
#endif
            }
        }

        void GenerateResourceChunks()
        {
            Vector3Int currentCoord = playerChunkCoord;
#if UNITY_EDITOR
            if (!Application.isPlaying) currentCoord = camChunkCoord;
#endif

            /// Unload chunks outside the render distance
            foreach (var kv in currentChunks.ToList())
            {
                var chunk = kv.Value;

                if (Mathf.Abs(chunk.Coord.x - currentCoord.x) > renderDistance ||
                    // Mathf.Abs(chunk.Coord.y - currentCoord.y) > renderDistance ||
                    Mathf.Abs(chunk.Coord.z - currentCoord.z) > renderDistance)
                {
                    chunk.Unload();
                    currentChunks.Remove(kv.Key);
                    print($"Unloaded resource chunk ({chunk.Coord.x}, {chunk.Coord.z})");
                }
            }
            
            /// Load chunks within the render distance
            for (int x = -renderDistance; x < renderDistance; x++)
            {
                // for (int y = -renderDistance; y < renderDistance; y++)
                // {
                    for (int z = -renderDistance; z < renderDistance; z++)
                    {
                        var chunkCoord = new Vector3Int(
                            currentCoord.x + x,
                            0,
                            currentCoord.z + z
                        );
                
                        if (currentChunks.ContainsKey(chunkCoord)) continue; /// chunk is already present
                        
                        if (loadedChunks.TryGetValue(chunkCoord, out ResourceChunkSaveData chunkData)) /// chunk was already generated before
                        {
                            
                        }
                        else /// new generated chunk
                        {
                            currentChunks[chunkCoord] = CreateChunk(chunkCoord);
                        }
                    // }
                }
            }
            
            chunksLoaded = currentChunks.Count;
        }

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

        ResourceChunk CreateChunk(Vector3Int coord)
        {
            var chunk = new GameObject($"Chunk ({coord.x}, {coord.y}, {coord.z})", typeof(ResourceChunk)).GetComponent<ResourceChunk>();
            chunk.transform.SetParent(transform);
            chunk.transform.position = new Vector3(
                coord.x * chunkSizeInt + (chunkSizeInt / 2),
                coord.y * chunkSizeInt + (chunkSizeInt / 2),
                coord.z * chunkSizeInt + (chunkSizeInt / 2));
            chunk.ChunkSize = chunkSizeInt;
            chunk.Coord = coord;
#if UNITY_EDITOR
                chunk.SetSeed(this.Seed);
#else
                chunk.SetSeed(World.GetSeed());
#endif
            chunk.SetTreeNoiseParameters(treeNoiseParameters);
            chunk.SetPickupNoiseParameters(pickupNoiseParameters);

            var generationSettings = new GenerateResourceChunkSettings()
            {
                PlaceTrees = PlaceTrees,
                PlacePickups = PlacePickups,
            };
            chunk.SetGenerationSettings(generationSettings);
            chunk.GenerateResources();
            print($"Created resource chunk at ({coord.x}, {coord.y}, {coord.z})");

            return chunk;
        }
        
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
            // if (enableLODChunking) /// buggy
            // {
            //     maxRenderDistance = renderDistance * 2; // Extend LOD visibility range
            // }
            // else
            // {
                maxRenderDistance = renderDistance;
            // }

            for (int z = -maxRenderDistance; z <= maxRenderDistance; z++)
            {
                // for (int y = -maxRenderDistance; z <= maxRenderDistance; z++)
                // {
                    for (int x = -maxRenderDistance; x <= maxRenderDistance; x++)
                    {
                        // if (enableLODChunking) /// buggy
                        // {
                        //     int lodSize = GetLODSize(x, z, chunkSizeInt);
                        //     Vector3Int chunkCoord = new(camChunkCoord.x + x, camChunkCoord.z + z);
                        //     Vector3Int alignedCoord = AlignToLODGrid(chunkCoord, lodSize);
                        //     Vector3 chunkPos = new(
                        //         alignedCoord.x * lodSize + (lodSize / 2f),
                        //         0,
                        //         alignedCoord.z * lodSize + (lodSize / 2f)
                        //     );
                        //     DrawLODChunk(chunkPos, lodSize);
                        // }
                        // else
                        // {
                            Vector3 chunkPos = new(
                                (camChunkCoord.x + x) * chunkSizeInt + (chunkSizeInt / 2), 
                                0, 
                                (camChunkCoord.z + z) * chunkSizeInt + (chunkSizeInt / 2)
                            );

                            DrawChunkGrid(chunkPos, chunkSizeInt);
                        // }
                    }
                // }
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
            loadedChunks.Clear();
        }
#endif
#endregion
    }
}