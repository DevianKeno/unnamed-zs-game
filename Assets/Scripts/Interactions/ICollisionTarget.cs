using System;

namespace UZSG.Interactions
{
    public interface ICollisionTarget
    {
        /// <summary>
        /// Called when this object is hit by an ICollisionSource.
        /// </summary>
        public event EventHandler<HitboxCollisionInfo> OnHit;
        public void HitBy(HitboxCollisionInfo info);
    }
}