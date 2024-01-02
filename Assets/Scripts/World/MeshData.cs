// using System;
// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using UZSG.Systems;

// namespace UZSG.WorldBuilder
// {
//     public class MeshData
//     {
//         public Mesh Mesh;
//         public int[] Indices;
//         public Vector3[] Vertices;
//         public Color[] Colors;

//         public int arraySize;

//         public void Initialize()
//         {
//             int maxTris = Game.World.Attributes.ChunkSize * Game.World.Attributes.WorldHeight * Game.World.Attributes.ChunkSize / 4;
//             arraySize = maxTris * 3;
//             Mesh = new Mesh();

//             Indices = new int[arraySize];
//             Vertices = new Vector3[arraySize];
//             Colors = new Color[arraySize];
//         }

//         public void ClearData()
//         {
//             Mesh.Clear();
//             GameObject.Destroy(Mesh);
//             Indices = null;
//             Vertices = null;
//             Colors = null;
//             Mesh = null;
//         }

//         public static readonly Vector3[] VoxelVertices = new Vector3[8]
//         {
//             new (0, 0, 0),    // 0
//             new (1, 0, 0),    // 1
//             new (0, 1, 0),    // 2
//             new (1, 1, 0),    // 3

//             new (0, 0, 1),    // 4
//             new (1, 0, 1),    // 5
//             new (0, 1, 1),    // 6
//             new (1, 1, 1),    // 7
//         };

//         public  static readonly Vector3[] VoxelFaceChecks = new Vector3[6]
//         {
//             new (0, 0, -1),   // Back
//             new (0, 0, 1),    // Front
//             new (-1, 0, 0),   // Left
//             new (1, 0, 0),    // Right
//             new (0, -1, 0),   // Bottom
//             new (0, 1, 0)     // Top
//         };

//         public static readonly int[,] VoxelVertexIndex = new int[6, 4]
//         {
//             {0, 1, 2, 3},
//             {4, 5, 6, 7},
//             {4, 0, 6, 2},
//             {5, 1, 7, 3},
//             {0, 1, 4, 5},
//             {2, 3, 6, 7},
//         };

//         public static readonly Vector2[] VoxelUVs = new Vector2[4]
//         {
//             new (0, 0),
//             new (0, 1),
//             new (1, 0),
//             new (1, 1)
//         };

//         public static readonly int[,] VoxelTris = new int[6, 6]
//         {
//             {0, 2, 3, 0, 3, 1},
//             {0, 1, 2, 1, 3, 2},
//             {0, 2, 3, 0, 3, 1},
//             {0, 1, 2, 1, 3, 2},
//             {0, 1, 2, 1, 3, 2},
//             {0, 2, 3, 0, 3, 1},
//         };
//     }
// }
