using UnityEngine;

namespace UZSG.Interactions
{
    public struct CollisionHitInfo
    {
        public ICollision By { get; set; }
        public Vector3 ContactPoint { get; set; }
    }
}