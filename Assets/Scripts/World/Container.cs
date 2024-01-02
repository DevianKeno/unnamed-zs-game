// using UnityEngine;
// using UZSG.Systems;

// namespace UZSG.WorldBuilder
// {
//     [RequireComponent(typeof(MeshFilter))]
//     [RequireComponent(typeof(MeshRenderer))]
//     [RequireComponent(typeof(MeshCollider))]
//     public class Container : MonoBehaviour
//     {
//         public Vector3 Position;
//         public MeshData MeshData;

//         MeshFilter _meshFilter;
//         MeshRenderer _meshRenderer;
//         MeshCollider _meshCollider;

//         void ConfigureComponents()
//         {
//             _meshFilter = GetComponent<MeshFilter>();
//             _meshRenderer = GetComponent<MeshRenderer>();
//             _meshCollider = GetComponent<MeshCollider>();
//         }  

//         public void Initialize(Material material, Vector3 position)
//         {
//             ConfigureComponents();            
//             MeshData = new();
//             MeshData.Initialize();
//             _meshRenderer.sharedMaterial = material;
//             Position = position;
//         }

//         public void ClearData()
//         {
//             _meshFilter.sharedMesh = null;
//             _meshCollider.sharedMesh = null;
//             MeshData.ClearData();
//         }
        
//         public void Dispose()
//         {
//             MeshData.ClearData();
//         }

//         public bool IsVoxelSolid(Vector3 point)
//         {
//             if  (point.y < 0 ||
//                 (point.x > Game.World.Attributes.ChunkSize + 2) ||
//                 (point.z > Game.World.Attributes.ChunkSize + 2))
//                 return true;
//             else
//                 return this[point].IsSolid;
//         }

//         public void UploadMesh(MeshBuffer meshBuffer)
//         {
//             if (_meshRenderer == null) ConfigureComponents();

//             // Get the count of vertices/tris from the shader
//             int[] faceCount = new int[2] { 0, 0 };
//             meshBuffer.CountBuffer.GetData(faceCount);

//             // Get all of the meshData from the buffers to local arrays
//             meshBuffer.VertexBuffer.GetData(MeshData.Vertices, 0, 0, faceCount[0]);
//             meshBuffer.IndexBuffer.GetData(MeshData.Indices, 0, 0, faceCount[0]);
//             meshBuffer.ColorBuffer.GetData(MeshData.Colors, 0, 0, faceCount[0]);

//             // Assign the mesh
//             MeshData.Mesh = new Mesh();
//             MeshData.Mesh.SetVertices(MeshData.Vertices, 0, faceCount[0]);
//             MeshData.Mesh.SetIndices(MeshData.Indices, 0, faceCount[0], MeshTopology.Triangles, 0);
//             MeshData.Mesh.SetColors(MeshData.Colors, 0, faceCount[0]);

//             MeshData.Mesh.RecalculateNormals();
//             MeshData.Mesh.RecalculateBounds();
//             MeshData.Mesh.Optimize();
//             MeshData.Mesh.UploadMeshData(true);

//             _meshFilter.sharedMesh = MeshData.Mesh;
//             _meshCollider.sharedMesh = MeshData.Mesh;

//             if (!gameObject.activeInHierarchy) gameObject.SetActive(true);        
//         }        

//         public Voxel this[Vector3 index]
//         {
//             get
//             {
//                 if (Game.World.modifiedVoxels.ContainsKey(Position))
//                 {
//                     if (Game.World.modifiedVoxels[Position].ContainsKey(index))
//                         return Game.World.modifiedVoxels[Position][index];
//                 }
//                 return new Voxel() { Id = 0 };
//             }

//             set
//             {
//                 if (!Game.World.modifiedVoxels.ContainsKey(Position))
//                     Game.World.modifiedVoxels.TryAdd(Position, new());

//                 if (!Game.World.modifiedVoxels[Position].ContainsKey(index))
//                     Game.World.modifiedVoxels[Position].Add(index, value);
//                 else
//                     Game.World.modifiedVoxels[Position][index] = value;
//             }
//         }
//     }
// }