using UnityEngine;

namespace UZSG.Interactions
{
    public struct CollisionHitInfo
    {
        public CollisionType Type { get; set; }
        public ICollisionSource Source { get; set; }
        public ICollisionTarget Target { get; set; }
        public Vector3 ContactPoint { get; set; }
        public Vector3 Inertia { get; set; }
    }
}