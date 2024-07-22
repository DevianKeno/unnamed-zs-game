using System;
using UnityEngine;

namespace UZSG.Entities
{
    [Serializable]
    public class Hitbox : MonoBehaviour
    {
        public HitboxPart Part;
        public event EventHandler<Collider> OnCollision;
        
        BoxCollider boxCollider;

        void Awake()
        {
            boxCollider = GetComponent<BoxCollider>();
        }

        public void Collision(Collider other)
        {
            OnCollision?.Invoke(this, other);
        }
    }
}
