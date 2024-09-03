using UnityEngine;

using UZSG.Systems;
using UZSG.Interactions;
using UZSG.Attributes;
using UZSG.Saves;

namespace UZSG.Entities
{
    /// <summary>
    /// Represents non-player Entities which may or may not have Hitboxes.
    /// </summary>
    public partial class NonPlayerCharacter : Entity
    {
        [Header("NPC Ragdoll Components")]
        public BoxCollider NonPlayerCollider;
        public GameObject NonPlayerBody;
        public Animator NonPlayerAnimator;
        public Rigidbody NonPlayerRigidbody;
        Collider[] _ragdollColliders;
        Rigidbody[] _rigidBodyParts;

        /// <summary>
        /// Store in arrays and variables all the NPC's collider, rigid body, and animator.
        /// </summary>
        void GetRagdollParts()
        {
            // Get components of ragdoll
            _ragdollColliders = NonPlayerBody.GetComponentsInChildren<Collider>();
            _rigidBodyParts = NonPlayerBody.GetComponentsInChildren<Rigidbody>();
            NonPlayerCollider = GetComponent<BoxCollider>();
            NonPlayerAnimator = GetComponentInChildren<Animator>();
            NonPlayerRigidbody = GetComponent<Rigidbody>();
        }

        /// <summary>
        /// Enable physics ragdoll for Entity.
        /// </summary>
        public void EnableRagdoll()
        {
            NonPlayerAnimator.enabled = false;
            NonPlayerCollider.enabled = false;
            NonPlayerRigidbody.isKinematic = true;

            foreach (var hitbox in hitboxController.Hitboxes)
            {
                hitbox.Collider.isTrigger = false;
                hitbox.Rigidbody.isKinematic = false;
            }
        }

        /// <summary>
        /// Disable physics ragdoll for Entity.
        /// </summary>
        public void DisableRagdoll()
        {
            NonPlayerAnimator.enabled = true;
            NonPlayerCollider.enabled = true;
            NonPlayerRigidbody.isKinematic = false;

            foreach (var hitbox in hitboxController.Hitboxes)
            {
                hitbox.Collider.isTrigger = true;
                hitbox.Rigidbody.isKinematic = true;
            }
        }
    }
}