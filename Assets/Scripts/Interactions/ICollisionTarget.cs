using System;

namespace UZSG.Interactions
{
    public interface ICollisionTarget
    {
        public event EventHandler<CollisionHitInfo> OnHit;
        public void HitBy(CollisionHitInfo info);
    }
}