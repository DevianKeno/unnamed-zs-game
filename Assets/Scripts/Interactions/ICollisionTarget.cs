using System;

namespace UZSG.Interactions
{
    [Serializable]
    public struct DamageContext
    {
        public float Value { get; set; }
        public bool Penetration { get; set; }
    }

    public interface ICollisionTarget
    {
        /// <summary>
        /// Called when this object is hit by an ICollisionSource.
        /// </summary>
        public event EventHandler<HitboxCollisionInfo> OnHit;
        public void HitBy(HitboxCollisionInfo info);
    }
}