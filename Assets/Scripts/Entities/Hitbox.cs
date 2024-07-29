using System;
using UnityEngine;

namespace UZSG.Entities
{
    public struct CollisionHitInfo
    {
        public ICollision By { get; set; }
        public Vector3 ContactPoint { get; set; }
    }

    [Serializable]
    public class Hitbox : MonoBehaviour
    {
        public HitboxPart Part;
        public event EventHandler<CollisionHitInfo> OnCollision;
        
        BoxCollider boxCollider;

        void Awake()
        {
            boxCollider = GetComponent<BoxCollider>();
        }

        public void HitBy(CollisionHitInfo other)
        {
            OnCollision?.Invoke(this, other);
        }
    }
}
