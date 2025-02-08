using System;
using System.Collections.Generic;
using System.Linq;


#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace UZSG.Worlds
{
    public class TerrainTextureHelper : MonoBehaviour
    {
        public List<Terrain> terrains = new();
        public List<TerrainLayerWithHeight> terrainLayers = new();
        public float BlendingRange = 5f;
        TerrainLayer[] _terrainLayersArray;
        Dictionary<Terrain, float[,,]> originalSplatmaps = new();

        public void CollectTerrains()
        {
            terrains.Clear();
            terrains.AddRange(GetComponentsInChildren<Terrain>());
            Debug.Log($"Found {terrains.Count} terrains in children.");
        }

        public void SaveOriginalSplatmaps()
        {
            originalSplatmaps.Clear();
            foreach (Terrain terrain in terrains)
            {
                TerrainData terrainData = terrain.terrainData;
                originalSplatmaps[terrain] = terrainData.GetAlphamaps(0, 0, terrainData.alphamapWidth, terrainData.alphamapHeight);
            }
        }

        public void ApplyTexturesToAll()
        {
            SaveOriginalSplatmaps();

            _terrainLayersArray = new TerrainLayer[terrainLayers.Count];
            for (int i = 0; i < terrainLayers.Count; i++)
            {
                _terrainLayersArray[i] = terrainLayers[i].Layer;
            }

            foreach (Terrain terrain in terrains)
            {
                terrain.terrainData.terrainLayers = _terrainLayersArray;
                ApplyTextures(terrain);
            }
        }

        void ApplyTextures(Terrain terrain)
        {
            TerrainData terrainData = terrain.terrainData;
            int width = terrainData.alphamapWidth;
            int height = terrainData.alphamapHeight;

            float[,,] splatmapData = new float[width, height, terrainData.terrainLayers.Length];

            for (int x = 0; x < terrainData.alphamapWidth; x++)
            {
                for (int y = 0; y < terrainData.alphamapHeight; y++)
                {
                    float terrainHeight = terrainData.GetHeight(y / 2, x / 2);

                    int topLayer = -1; 
                    int bottomLayer = -1; 

                    // Find the two closest layers: the one below (bottomLayer) and the one above (topLayer)
                    for (int i = 0; i < terrainLayers.Count; i++)
                    {
                        if (terrainHeight >= terrainLayers[i].Height)
                        {
                            bottomLayer = i; // This is the lower layer
                        }
                        else
                        {
                            topLayer = i; // This is the first layer above the terrain height
                            break; // Stop once we find the first layer above
                        }
                    }

                    // If the terrain is below all layers, fallback to the lowest layer
                    if (bottomLayer == -1)
                    {
                        bottomLayer = 0;
                    }

                    // If the terrain is above all layers, fallback to the highest layer
                    if (topLayer == -1)
                    {
                        topLayer = bottomLayer;
                    }

                    // Compute blending factor (0 to 1) between bottomLayer and topLayer
                    float blendFactor = 0f;
                    if (bottomLayer != topLayer) // Only blend if they are different
                    {
                        float minHeight = terrainLayers[bottomLayer].Height;
                        float maxHeight = terrainLayers[topLayer].Height;
                        blendFactor = Mathf.InverseLerp(minHeight, minHeight + BlendingRange, terrainHeight);
                    }

                    // Assign weights based on blend factor
                    for (int i = 0; i < terrainLayers.Count; i++)
                    {
                        if (i == bottomLayer)
                        {
                            splatmapData[x, y, i] = 1f - blendFactor; // More weight to lower layer
                        }
                        else if (i == topLayer)
                        {
                            splatmapData[x, y, i] = blendFactor; // More weight to upper layer
                        }
                        else
                        {
                            splatmapData[x, y, i] = 0f; // Other layers get no influence
                        }
                    }
                }
            }

    #if UNITY_EDITOR
            Undo.RegisterCompleteObjectUndo(terrainData, "Apply Terrain Texture");
    #endif

            terrainData.SetAlphamaps(0, 0, splatmapData);
        }

        public void UndoTextureChanges()
        {
            foreach (var kvp in originalSplatmaps)
            {
                Terrain terrain = kvp.Key;
                float[,,] originalSplatmap = kvp.Value;

                if (originalSplatmap == null)
                    continue;

                TerrainData terrainData = terrain.terrainData;

    #if UNITY_EDITOR
                Undo.RegisterCompleteObjectUndo(terrainData, "Undo Terrain Texture");
    #endif

                terrainData.SetAlphamaps(0, 0, originalSplatmap);
            }

            Debug.Log("All terrain textures reverted.");
        }
    }
}
