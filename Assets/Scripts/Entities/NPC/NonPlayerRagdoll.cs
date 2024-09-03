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
        public bool IsRagdollOff;
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

            // set to false since NPC is initially alive
            IsRagdollOff = true;
        }

        /// <summary>
        /// Set the npc's components to ragdoll mode on or off.
        /// </summary>
        /// <param name="enabled"></param>
        public void RagdollMode(bool enabled)
        {
            if (enabled)
            {

            }
            NonPlayerAnimator.enabled = enabled;

            // foreach (var hitbox in hitboxController.Hitboxes)
            // {
            //     hitbox.Rigidbody.isKinematic = IsTrue;
            // }

            foreach(Collider col in _ragdollColliders)
            {
                // col.enabled = !enabled;
                col.isTrigger = !enabled;
            }

            foreach(Rigidbody rigid in _rigidBodyParts)
            {
                rigid.isKinematic = enabled;
            }

            NonPlayerCollider.enabled = enabled;
            NonPlayerRigidbody.isKinematic = !enabled;
        }
    }
}   