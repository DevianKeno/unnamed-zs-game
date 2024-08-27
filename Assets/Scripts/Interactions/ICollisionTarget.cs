using System;

namespace UZSG.Interactions
{
    [Serializable]
    public struct DamageContext
    {
        public float Value { get; set; }
        public bool Penetration { get; set; }
    }

    /// <summary>
    /// Represents stuff that is targeted by collisions, which are ICollisionSources.
    /// </summary>
    public interface ICollisionTarget
    {
        /// <summary>
        /// Called when this object is hit by an ICollisionSource.
        /// </summary>
        public event EventHandler<HitboxCollisionInfo> OnHit;
        public void HitBy(HitboxCollisionInfo info);
    }
}