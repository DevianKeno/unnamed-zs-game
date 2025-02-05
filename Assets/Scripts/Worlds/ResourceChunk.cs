using System;
using System.Collections.Generic;

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
        public int Seed;
        [SerializeField] NoiseParameters treeNoiseParams;
        [SerializeField] NoiseParameters pickupsNoiseParams;

        int chunkSize;
        RaycastHit hit;
        [SerializeField] List<Resource> resources = new();

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
            this.Seed = seed;
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

        /// <summary>
        /// The terrain this chunk is in.
        /// </summary>
        Terrain terrain;

        public void GenerateResources(int chunkSize, Vector3Int chunkCoord, GenerateResourceChunkSettings settings)
        {
            this.chunkSize = chunkSize;
            int groundLayerMask = LayerMask.NameToLayer("Ground");
            float[,] treeNoiseMap = Noise.Generate2DRandom01(
                seed: Seed,
                width: chunkSize, height: chunkSize,
                offset: new (chunkCoord.x, chunkCoord.z), /// 2D chunk offset
                density01: settings.TreeDensity
            );

            float[,] pickupNoiseMap = Noise.Generate2DRandom01(
                seed: Seed,
                width: chunkSize, height: chunkSize,
                offset: new (chunkCoord.x, chunkCoord.z), /// 2D chunk offset
                density01: settings.PickupsDensity
            );

            var worldPos = new Vector3(this.transform.position.x, 8000f, this.transform.position.z);
            var ray = new Ray(worldPos, -Vector3.up);

            Debug.DrawRay(ray.origin, ray.direction, Color.red, 0.5f);

            if (!Physics.Raycast(ray, out hit, Mathf.Infinity, groundLayerMask))
            {
                Debug.LogWarning($"No terrain found for chunk ({chunkCoord.x}, {chunkCoord.y}, {chunkCoord.z})");
                return;
            }
            if (!hit.collider.TryGetComponent<Terrain>(out terrain))
            {
                Debug.LogWarning($"No Terrain component attached? Maybe an object hit?");
                return;
            };

            Vector3 samplePoint;
            /// NOTE: For loop assumes that all noise map lengths are the same :)
            for (int z = 0; z < treeNoiseMap.GetLength(1); z++)
            {
                for (int x = 0; x < treeNoiseMap.GetLength(0); x++)
                {
                    samplePoint = new Vector3(
                        x + this.transform.position.x,
                        0f,
                        z + this.transform.position.x
                    );
                    var y = terrain.SampleHeight(samplePoint);
                    samplePoint.y = y;

                    Handles.DrawWireCube(samplePoint, size: Vector3.one);

                    var layer = GetTerrainLayerFromPoint(terrain, hit.point);

                    if (treeNoiseMap[x, z] > settings.TreeDensity &&
                        settings.PlaceTrees)
                    {
                        TryPlaceTree(layer, hit.point);
                    }
                    if (pickupNoiseMap[x, z] > settings.PickupsDensity &&
                        settings.PlacePickups)
                    {
                        TryPlacePickup(layer, hit.point);
                    }
                }
            }
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
}