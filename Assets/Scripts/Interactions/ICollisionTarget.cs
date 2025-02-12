using System;

namespace UZSG.Interactions
{
    /// <summary>
    /// Represents stuff that is targeted by collisions, which are ICollisionSources.
    /// </summary>
    public interface ICollisionTarget
    {
        /// <summary>
        /// Called when this object is hit by an ICollisionSource.
        /// </summary>
        public event EventHandler<HitboxCollisionInfo> OnCollision;
        public void HitBy(HitboxCollisionInfo info);
    }
}