using UnityEngine;

namespace UZSG.Interactions
{
    public struct HitboxCollisionInfo
    {
        public ICollisionSource Source { get; set; }
        public ICollisionTarget Target { get; set; }
        public CollisionType Type { get; set; }
        public Collider Collider { get; set; }
        public Vector3 ContactPoint { get; set; }
        public Vector3 Inertia { get; set; }
    }
}