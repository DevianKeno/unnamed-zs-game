using System;
using UnityEngine;

namespace UZSG.Entities
{
    public struct CollisionHitInfo
    {
        public ICollision By { get; set; }
        public Vector3 ContactPoint { get; set; }
    }
}