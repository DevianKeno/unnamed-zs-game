using System;

using UnityEngine;

using UZSG.Interactions;

namespace UZSG.Entities
{
    [Serializable]
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(Collider))]
    public class Hitbox : MonoBehaviour, ICollisionTarget
    {
        public HitboxPart Part;
        public event EventHandler<HitboxCollisionInfo> OnCollision;

        Collider coll;
        public Collider Collider => coll;
        Rigidbody rb;
        public Rigidbody Rigidbody => rb;
        public bool HasJoint;
        CharacterJoint joint;
        public CharacterJoint CharacterJoint => joint;

        void Awake()
        {
            InitializeComponents();
        }

        public void InitializeComponents()
        {
            coll = GetComponent<Collider>();
            rb = GetComponent<Rigidbody>();
            HasJoint = TryGetComponent(out joint);
        }

        public void HitBy(HitboxCollisionInfo other)
        {
            OnCollision?.Invoke(this, other); 
        }
    }
}
