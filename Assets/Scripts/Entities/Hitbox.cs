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
        public BoxCollider Collider => boxCollider;
        Rigidbody rb;
        public Rigidbody Rigidbody => rb;
        CharacterJoint joint;
        public CharacterJoint CharacterJoint => joint;

        void Awake()
        {
            boxCollider = GetComponent<BoxCollider>();
            rb = GetComponent<Rigidbody>();
            joint = GetComponent<CharacterJoint>();
        }

        public void HitBy(HitboxCollisionInfo other)
        {
            OnHit?.Invoke(this, other);
        }
    }
}
