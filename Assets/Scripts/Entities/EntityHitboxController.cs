using System.Collections.Generic;

using UnityEngine;

namespace UZSG.Entities
{
    public class EntityHitboxController : MonoBehaviour
    {
        [SerializeField] List<Hitbox> hitboxes;
        public List<Hitbox> Hitboxes => hitboxes;

        public bool IsTrigger;
        public CollisionDetectionMode CollisionDetection = CollisionDetectionMode.Continuous;
        /// supposed to be LayerMask but damn
        public string Layer = "Hitbox";
        public LayerMask IncludeLayers;
        public LayerMask ExcludeLayers;

        public void ReinitializeHitboxes()
        {
            hitboxes = new();
            GetHitboxFromChildrenRecursive(transform);
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }

        void GetHitboxFromChildrenRecursive(Transform transform)
        {
            foreach (Transform child in transform)
            {
                if (child.TryGetComponent<Hitbox>(out var hitbox))
                {
                    hitbox.InitializeComponents();
                    if (hitbox.Collider != null)
                    {
                        hitbox.Collider.isTrigger = IsTrigger; /// true before
                        hitbox.Collider.includeLayers = IncludeLayers;
                        hitbox.Collider.excludeLayers = ExcludeLayers;
                    }
                    if (hitbox.Rigidbody != null)
                    {
                        hitbox.Rigidbody.collisionDetectionMode = CollisionDetection;
                        hitbox.Rigidbody.includeLayers = IncludeLayers;
                        hitbox.Rigidbody.excludeLayers = ExcludeLayers;
                    }
                    hitbox.gameObject.layer = LayerMask.NameToLayer(Layer);
                    hitboxes.Add(hitbox);
                }
                GetHitboxFromChildrenRecursive(child);
            }
        }
    }
}