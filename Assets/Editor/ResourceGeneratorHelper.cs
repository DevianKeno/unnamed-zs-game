using UnityEditor;
using UnityEngine;

namespace UZSG.Worlds
{
    [RequireComponent(typeof(World))]
    public class ResourceGeneratorHelper : MonoBehaviour
    {
        public GameObject Target;

        [Range(0, 1000)]
        public int InstancesCount;
        public LayerMask Layers;
        public GameObject Parent;

        [Header("Positions")]
        public Vector3 StartPosition;
        public Vector3 EndPosition;
        public Vector3 Offset;
        public bool UseTerrainSurfaceAsY;

        [Header("Rotations")]
        public bool RandomizeRotations = true;
        public Vector3 StartRotation;
        public Vector3 EndRotation;

        [Header("Settings")]
        public float SkyRaycastHeight = 255f;
        public float RaycastLength = 999f;


#if UNITY_EDITOR

        public void PlaceTargetInstances()
        {
            for (int i = 0; i < InstancesCount; i++)
            {
                var go = (GameObject) PrefabUtility.InstantiatePrefab(Target, Parent != null ? Parent.transform : transform.Find("OBJECTS"));
                
                /// Randomize position
                float posX = UnityEngine.Random.Range(StartPosition.x, EndPosition.x);
                float posY = UnityEngine.Random.Range(StartPosition.y, EndPosition.y);
                float posZ = UnityEngine.Random.Range(StartPosition.z, EndPosition.z);
                if (UseTerrainSurfaceAsY)
                {
                    if (Physics.Raycast(new(posX, SkyRaycastHeight, posZ), -Vector3.up, out var hit, RaycastLength, Layers))
                    {
                        if (hit.collider.TryGetComponent<Terrain>(out var terrain))
                        {
                            posY = hit.point.y;
                        }
                    }
                }
                else
                {
                    Debug.LogWarning($"[ResourceGeneratorHelper]: No surface found at coordinate ({posX}, {posY}, {posZ}), skipping {i}");
                    continue;
                }

                go.transform.position = new Vector3(posX, posY, posZ) + Offset;

                /// Randomize rotations
                if (RandomizeRotations)
                {
                    float rotX = UnityEngine.Random.Range(StartRotation.x, EndRotation.x);
                    float rotY = UnityEngine.Random.Range(StartRotation.y, EndRotation.y);
                    float rotZ = UnityEngine.Random.Range(StartRotation.z, EndRotation.z);
                    go.transform.rotation = Quaternion.Euler(rotX, rotY, rotZ);
                }
                
                Debug.Log($"[ResourceGeneratorHelper]: Placed new object {Target.name} at ({posX}, {posY}, {posZ})");
            }
        }
#endif
    }
}