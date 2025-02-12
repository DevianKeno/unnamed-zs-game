// #define DEBUGGING_ENABLED

using System;
using System.Collections.Generic;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

using UZSG.Data;
using UZSG.Entities;
using UZSG.Saves;

namespace UZSG.Worlds
{
    /// <summary>
    /// Dynamically loads resource objects in the world.
    /// </summary>
#if UNITY_EDITOR
    [ExecuteInEditMode]
#endif
    public class ChunkManager : MonoBehaviour
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
        
        [SerializeField] ChunkSizes chunkSize = ChunkSizes.Full;
        int chunkSizeInt => (int) chunkSize;
        [SerializeField] internal bool enableLoadingChunks = false;

        [Header("Generation Settings")]
        [Header("Trees")]
        public int Seed = 12345;
        public bool PlaceTrees = true;
        [SerializeField] NoiseData treesNoiseData;

        [Header("Pickups")]
        public bool PlacePickups = true;
        [SerializeField] NoiseData pickupsNoiseData;

        [Header("Ore Deposits")]
        public bool PlaceOreDeposits = true;
        [SerializeField] NoiseData oreDepositsNoiseData;

        [Header("Status")]
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
        Dictionary<Vector3Int, Chunk> currentLoadedChunks = new();
        /// <summary>
        /// Chunks that are saved in memory, but is not loaded.
        /// </summary>
        Dictionary<Vector3Int, ChunkSaveData> saveDataChunks = new();

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
            currentLoadedChunks.Clear();
            saveDataChunks.Clear();
        }

        internal void Deinitialize()
        {
            enableLoadingChunks = false;
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
            enableLoadingChunks = true;
        }


        void Update()
        {
// #if UNITY_EDITOR
//                 if (SceneView.lastActiveSceneView == null) return;
            
//                 Vector3 camPos = SceneView.lastActiveSceneView.camera.transform.position;
//                 camChunkCoord = ToChunkCoord(camPos);

//                 if (camChunkCoord != lastCamChunkCoord)
//                 {
//                     GenerateChunks();
//                     lastCamChunkCoord = camChunkCoord;
//                 }
// #endif
        }

        void FixedUpdate()
        {
            if (!Application.isPlaying || !enableLoadingChunks) return;

            playerChunkCoord = ToChunkCoord(player.Position);

            // Regenerate chunks if the player moves to a new chunk
            if (playerChunkCoord != lastPlayerChunkCoord)
            {
                GenerateChunks();
                lastPlayerChunkCoord = playerChunkCoord;
            }
        }

        public bool ChunkExistsAt(Vector3Int coord)
        {
            return currentLoadedChunks.ContainsKey(coord);
        }

        /// <summary>
        /// Takes a world position and converts it to the chunk coordinate it's in.
        /// </summary>
        public Vector3Int ToChunkCoord(Vector3 position)
        {
            return new Vector3Int(
                Mathf.FloorToInt(position.x / chunkSizeInt),
                Mathf.FloorToInt(position.y / chunkSizeInt),
                Mathf.FloorToInt(position.z / chunkSizeInt)
            );
        }

        public Chunk GetChunkAt(Vector3Int chunkCoord)
        {
            currentLoadedChunks.TryGetValue(chunkCoord, out var chunk);
            return chunk;
        }

        /// <summary>
        /// Takes an array of three int values to chunk coordinates.
        /// </summary>
        public static Vector3Int ToChunkCoord(int[] values)
        {
            return new(values[0], values[1], values[2]);
        }

        internal List<ChunkSaveData> SaveChunks()
        {
            List<ChunkSaveData> chunkSaves = new();
            List<Vector3Int> alreadyWritten = new();
            
            /// save currently loaded chunks
            foreach (var chunk in currentLoadedChunks.Values)
            {
                if (!chunk.IsDirty) continue;
                
                chunkSaves.Add(chunk.WriteSaveData());
                alreadyWritten.Add(chunk.Coord);
            }
            /// save the remaining chunks that were loaded in memory
            foreach (var chunkSave in saveDataChunks.Values)
            {
                if (alreadyWritten.Contains(ToChunkCoord(chunkSave.Coord))) continue;                
                chunkSaves.Add(chunkSave);
            }
            
            alreadyWritten.Clear();
            return chunkSaves;
        }

        /// <summary>
        /// Reads the saved chunks and loads it into memory.
        /// </summary>
        internal void ReadChunks(List<ChunkSaveData> chunkSaves)
        {
            foreach (var chunkSave in chunkSaves)
            {
                var coord = ToChunkCoord(chunkSave.Coord); /// from int[3]                
                saveDataChunks[coord] = chunkSave;
            }
        }

        void GenerateChunks()
        {
            Vector3Int currentCoord = playerChunkCoord;
#if UNITY_EDITOR
            if (!Application.isPlaying) currentCoord = camChunkCoord;
#endif

            /// Unload chunks outside the render distance
            foreach (var kv in currentLoadedChunks.ToList())
            {
                var chunk = kv.Value;

                if (Mathf.Abs(chunk.Coord.x - currentCoord.x) > renderDistance ||
                    // Mathf.Abs(chunk.Coord.y - currentCoord.y) > renderDistance ||
                    Mathf.Abs(chunk.Coord.z - currentCoord.z) > renderDistance)
                {
                    if (chunk.IsDirty)
                    {
                        saveDataChunks[chunk.Coord] = chunk.WriteSaveData();
                    }
                    chunk.Unload();
                    currentLoadedChunks.Remove(kv.Key);
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
                
                        if (currentLoadedChunks.ContainsKey(chunkCoord)) continue; /// chunk is already present
                        
                        if (saveDataChunks.TryGetValue(chunkCoord, out ChunkSaveData chunkSave)) /// chunk was already generated before
                        {
                            var chunk = CreateEmptyChunk(chunkCoord);
                            currentLoadedChunks[chunkCoord] = chunk;

                            chunk.SetSeed(World.GetSeed());
                            chunk.ReadSaveData(chunkSave);
                        }
                        else /// new generated chunk
                        {
                            currentLoadedChunks[chunkCoord] = CreateNewChunkWithResources(chunkCoord);
                        }
                    // }
                }
            }
            
            chunksLoaded = currentLoadedChunks.Count;
        }

        Chunk CreateNewChunkWithResources(Vector3Int coord)
        {
            var chunk = CreateEmptyChunk(coord);
            chunk.SetSeed(World.GetSeed());
            chunk.treesNoiseData = treesNoiseData; /// TODO: read from world aatributes
            chunk.pickupsNoiseData = pickupsNoiseData; /// TODO: read from world aatributes
            chunk.oreDepositsNoiseData = oreDepositsNoiseData; /// TODO: read from world aatributes

            var generationSettings = new GenerateResourceChunkSettings()
            {
                PlaceTrees = PlaceTrees,
                PlacePickups = PlacePickups,
                PlaceOreDeposits = PlaceOreDeposits,
            };
            chunk.SetGenerationSettings(generationSettings);
            chunk.GenerateResources();

            return chunk;
        }

        /// <summary>
        /// Creates an empty chunk given the chunk coord.
        /// </summary>
        Chunk CreateEmptyChunk(Vector3Int coord)
        {
            var chunk = new GameObject($"Chunk ({coord.x}, {coord.y}, {coord.z})", typeof(Chunk)).GetComponent<Chunk>();
            chunk.transform.SetParent(transform);
            chunk.transform.position = new Vector3(
                coord.x * chunkSizeInt + (chunkSizeInt / 2),
                coord.y * chunkSizeInt + (chunkSizeInt / 2),
                coord.z * chunkSizeInt + (chunkSizeInt / 2));
            chunk.ChunkSize = chunkSizeInt;
            chunk.Coord = coord;
            print($"Created empty chunk at ({coord.x}, {coord.y}, {coord.z})");
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
        void OnDrawGizmosSelected()
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
            saveDataChunks.Clear();
        }
#endif
        #endregion
    }
}