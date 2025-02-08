using System.Collections.Generic;
using UnityEngine;

namespace UZSG.Worlds
{
    public class TerrainAligner : MonoBehaviour
    {
        public Vector3 SizeOverride = new(512, 1536, 512);
        public List<Terrain> terrains = new();
        
        [ContextMenu("Collect Terrains")]
        public void CollectTerrains()
        {
            terrains.Clear();
            terrains.AddRange(GetComponentsInChildren<Terrain>());
            Debug.Log($"Found {terrains.Count} terrains in children.");
        }

        [ContextMenu("Override Terrain Size All")]
        public void OverrideTerrainSizeAll()
        {
            foreach (Terrain terr in terrains)
            {
                terr.terrainData.size = SizeOverride;
                terr.gameObject.AddComponent<TerrainChunk>();
            }
        }
        
        [ContextMenu("GetCoords")]
        public void GetCoords()
        {
            foreach (Terrain terr in terrains)
            {
                var terrChunk = terr.GetComponent<TerrainChunk>();
                terrChunk.GetCoords();
            }
        }
    }
}