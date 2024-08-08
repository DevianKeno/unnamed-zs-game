using UnityEngine;

namespace UZSG.Interactions
{
    public struct CollisionHitInfo
    {
        public ICollisionSource Source { get; set; }
        public Vector3 ContactPoint { get; set; }
        public Vector3 Inertia { get; set; }
    }
}