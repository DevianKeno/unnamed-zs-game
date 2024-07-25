using System;
using UnityEngine;

namespace UZSG.Entities
{
    public struct CollisionHitInfo
    {
        public IProjectile Projectile { get; set; }
        public RaycastHit Hit { get; set; }
    }

    [Serializable]
    public class Hitbox : MonoBehaviour
    {
        public HitboxPart Part;
        public event EventHandler<Collider> OnCollision;
        
        BoxCollider boxCollider;

        void Awake()
        {
            boxCollider = GetComponent<BoxCollider>();
        }

        public void Hit(Collider other)
        {
            OnCollision?.Invoke(this, other);
        }
    }
}
