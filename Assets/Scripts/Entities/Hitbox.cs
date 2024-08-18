using System;

using UnityEngine;

using UZSG.Interactions;

namespace UZSG.Entities
{
    [Serializable]
    public class Hitbox : MonoBehaviour, ICollisionTarget
    {
        public HitboxPart Part;
        public event EventHandler<HitboxCollisionInfo> OnHit;

        BoxCollider boxCollider;

        void Awake()
        {
            boxCollider = GetComponent<BoxCollider>();
        }

        public void HitBy(HitboxCollisionInfo other)
        {
            OnHit?.Invoke(this, other);
        }
    }
}
