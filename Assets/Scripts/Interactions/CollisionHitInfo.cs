using UnityEngine;

namespace UZSG.Interactions
{
    public enum HitboxCollisionType {
        Collision, Attack,
    }

    public struct HitboxCollisionInfo
    {
        public HitboxCollisionType CollisionType { get; set; }
        public ICollisionSource Source { get; set; }
        public ICollisionTarget Target { get; set; }
        public ObjectCollisionType ObjectType { get; set; }
        public Collider Collider { get; set; }
        public Vector3 ContactPoint { get; set; }
        public Vector3 ContactNormal { get; set; }
        public Vector3 Velocity { get; set; }
    }
}