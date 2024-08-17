using System;

namespace UZSG.Interactions
{
    public interface ICollisionTarget
    {
        /// <summary>
        /// Called when this object is hit by an ICollisionSource.
        /// </summary>
        public event EventHandler<CollisionHitInfo> OnHit;
        public void HitBy(CollisionHitInfo info);
    }
}