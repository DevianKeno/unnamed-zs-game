using System;
using System.Collections.Generic;
using UnityEngine;

namespace UZSG.Worlds
{
    public class TerrainChunk : MonoBehaviour
    {
        public Vector3Int Coord;

        [SerializeField] Terrain terrain;
        

        [ContextMenu("Get Terrain Component")]
        public void GetTerrainComponent()
        {
            terrain = GetComponent<Terrain>();
        }

        [ContextMenu("Align to Coords")]
        public void AlignToCoord()
        {
            transform.position = new(
                Coord.x * terrain.terrainData.size.x,
                Coord.y * terrain.terrainData.size.y,
                Coord.z * terrain.terrainData.size.z);
        }
        
        [ContextMenu("Get Coords")]
        public void GetCoords()
        {
            Coord = new Vector3Int(
                (int)(transform.position.x / 500),
                (int)(transform.position.y / 500),
                (int)(transform.position.z / 500));
        }
    }
}
