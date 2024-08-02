using System;
using UnityEngine;

namespace UZSG.Entities
{
    [Serializable]
    public class Hitbox : MonoBehaviour
    {
        public HitboxPart Part;
        public event EventHandler<CollisionHitInfo> OnCollision;
        
        BoxCollider boxCollider;

        void Awake()
        {
            boxCollider = GetComponent<BoxCollider>();
        }

        public void HitBy(CollisionHitInfo other)
        {
            OnCollision?.Invoke(this, other);
        }
    }
}
