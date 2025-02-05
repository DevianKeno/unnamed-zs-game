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

using UZSG.Entities;

namespace UZSG.Worlds
{
    /// <summary>
    /// Dynamically loads resource objects in the world.
    /// </summary>
    [ExecuteInEditMode]
    public class ResourceChunkManagerEditor : MonoBehaviour
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
        
        public bool EnableLoadingChunks = true;
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
        /// Loaded chunks.
        /// <c>Vector3Int</c> is the chunk coordinate.
        /// </summary>
        Dictionary<Vector3Int, ResourceChunk> loadedChunks = new();

        [SerializeField] Transform chunksContainer;

        [Header("Debugging")]
        [Range(0, 1f)] public float chunkOpacity = 0.5f;
        [SerializeField] bool enableLODChunking = false;
        Color chunkColorLOD0 = new(0, 1, 0); // Green (LOD 0)
        Color chunkColorLOD1 = new(1, 1, 0); // Yellow (LOD 1)
        Color chunkColorLOD2 = new(1, 0, 0); // Red (LOD 2)

        internal void Initialize(World world)
        {   
            if (world == null)
            {
                Game.Console.LogError($"[World::ResourceManager]: Must be initialized given a world!");
                return;
            }
            var player = World.GetLocalPlayer();
            if (player == null)
            {
                Game.Console.LogError($"[World::ResourceManager]: No local player found!");
                return;
            }
            this.player = player;
            loadedChunks.Clear();
        }

        void OnValidate()
        {
            treeNoiseParameters.Seed = this.Seed;
            pickupNoiseParameters.Seed = this.Seed;
        }

        void Update()
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
            currentCoord = camChunkCoord;
#endif

            /// Unload chunks outside the render distance
            foreach (var kv in loadedChunks.ToList())
            {
                var chunk = kv.Value;

                if (Mathf.Abs(chunk.Coord.x - currentCoord.x) > renderDistance ||
                    // Mathf.Abs(chunk.Coord.y - currentCoord.y) > renderDistance ||
                    Mathf.Abs(chunk.Coord.z - currentCoord.z) > renderDistance)
                {
                    chunk.Unload();
                    loadedChunks.Remove(kv.Key);
                    print($"Unloaded resource chunk ({chunk.Coord.x}, {chunk.Coord.z})");
                }
            }
            
            /// Load chunks within the render distance
            for (int x = -renderDistance; x < renderDistance; x++)
            {
                for (int z = -renderDistance; z < renderDistance; z++)
                {
                    var chunkCoord = new Vector3Int(
                        currentCoord.x + x,
                        0,
                        currentCoord.z + z
                    );

                    if (!ChunkExistsAt(chunkCoord))
                    {
                        loadedChunks[chunkCoord] = CreateChunk(chunkCoord);
                    }
                }
            }
        }

        bool ChunkExistsAt(Vector3Int coord)
        {
            return loadedChunks.ContainsKey(coord);
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
            var chunk = new GameObject($"Resource Chunk ({coord.x}, {coord.z})", typeof(ResourceChunk)).GetComponent<ResourceChunk>();
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
            print($"Created resource chunk at ({coord.x}, {coord.z})");

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
            loadedChunks.Clear();
        }
#endif
#endregion
    }

    [BurstCompile]
    public struct ChunkLoadJob : IJob
    {
        public void Execute()
        {
            
        }
    }
}