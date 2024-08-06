using UnityEngine;
using UnityEngine.UIElements;
using UZSG.Entities;
using UZSG.Interactions;

namespace UZSG.Players
{
    public class GroundChecker : MonoBehaviour
    {
        [SerializeField] bool _isGrounded;
        public LayerMask mask;
        public Transform rayStart;
        public float rayCastRange;

        RaycastHit hit;
        Material material;
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
            GroundMaterialDetection();
        }

        public void OnTriggerExit(Collider other)
        {
            _isGrounded = false;
        }

        public void GroundMaterialDetection()
        {
            if (Physics.Raycast(rayStart.position, rayStart.transform.up * -1 , out hit, rayCastRange, mask))
            {
                MeshCollider collider = hit.collider as MeshCollider;
                // Remember to handle case where collider is null because you hit a non-mesh primitive...

                if(collider == null)
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