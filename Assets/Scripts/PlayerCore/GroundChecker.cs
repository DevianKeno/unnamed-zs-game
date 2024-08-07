using System;
using UnityEngine;
using UnityEngine.UIElements;
using UZSG.Entities;
using UZSG.Interactions;

namespace UZSG.Players
{
    public class GroundChecker : MonoBehaviour
    {
        [SerializeField] bool _isGrounded;
        [SerializeField] bool _blendTerrainSounds;
        public bool drawRayCast;
        public LayerMask mask;
        public Transform rayStart;
        public float rayCastRange;

        RaycastHit hit;
        Material material;
        string _texture;
        int _limit;
        int _submesh;
        int _numIndices;

        public bool IsGrounded => _isGrounded;

        [SerializeField] BoxCollider boxCollider;

        public void OnTriggerEnter(Collider other)
        {
            _isGrounded = true;
        }

        public void OnTriggerStay(Collider other)
        {
            _isGrounded = true;
            GroundTextureDetection();
        }

        public void OnTriggerExit(Collider other)
        {
            _isGrounded = false;
        }

        public void GroundTextureDetection()
        {
            
            if (Physics.Raycast(rayStart.position, -rayStart.up, out hit, rayCastRange, mask))
            {
                if (drawRayCast)
                {
                    Debug.DrawLine(rayStart.position, hit.point, Color.red);
                }
                
                if (hit.collider.TryGetComponent<Terrain>(out Terrain _terrain))
                {
                    GetTexture(_terrain, hit.point);
                }
                else if (hit.collider.TryGetComponent<Renderer>(out Renderer _renderer))
                {
                    GetRenderMaterial(_renderer);
                }
            }
            else
            {
                if (drawRayCast)
                {
                    Debug.DrawLine(rayStart.position, hit.point, Color.blue);
                }
            }
        }

        private void GetTexture(Terrain terrain, Vector3 hitPoint)
        {
            Vector3 _terrainPosition = hitPoint - terrain.transform.position;
            Vector3 _splatMapPosition = new Vector3(_terrainPosition.x / terrain.terrainData.size.x, 0, _terrainPosition.z / terrain.terrainData.size.z);

            int x = Mathf.FloorToInt(_splatMapPosition.x * terrain.terrainData.alphamapWidth);
            int z = Mathf.FloorToInt(_splatMapPosition.z * terrain.terrainData.alphamapHeight);

            float[,,] _alphaMap = terrain.terrainData.GetAlphamaps(x, z, 1, 1);

            if (!_blendTerrainSounds)
            {
                int _primaryIndex = 0;
                for (int i = 1; i < _alphaMap.Length; i++)
                {
                    if (_alphaMap[0, 0, i] > _alphaMap[0, 0, _primaryIndex])
                    {
                        _primaryIndex = i;
                    }
                }

                _texture = terrain.terrainData.terrainLayers[_primaryIndex].diffuseTexture.name;
            }
        }

        private void GetRenderMaterial(Renderer renderer) 
        {
            _texture = renderer.material.GetTexture("_MainTex").name;
        }

        // not in currently in use, but might be helpful in future
        public void GroundMaterialDetection()
        {
            if (Physics.Raycast(rayStart.position, rayStart.transform.up * -1, out hit, rayCastRange, mask))
            {
                MeshCollider collider = hit.collider as MeshCollider;
                // Remember to handle case where collider is null because you hit a non-mesh primitive...

                if (collider == null)
                {
                    material = null;
                }
                else
                {
                    Mesh mesh = collider.sharedMesh;

                    // There are 3 indices stored per triangle
                    _limit = hit.triangleIndex * 3;

                    for (_submesh = 0; _submesh < mesh.subMeshCount; _submesh++)
                    {
                        _numIndices = mesh.GetTriangles(_submesh).Length;
                        if (_numIndices > _limit)
                            break;

                        _limit -= _numIndices;
                    }

                    material = collider.GetComponent<MeshRenderer>().sharedMaterials[_submesh];
                }
            }
        }
    }
}