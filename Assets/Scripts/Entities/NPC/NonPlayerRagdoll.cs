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
        [SerializeField] BoxCollider ragdollCollider;
        [SerializeField] GameObject ragdollBody;
        [SerializeField] Animator ragdollAnimator;
        [SerializeField] Rigidbody ragdollRigidbody;
        Collider[] _ragdollColliders;
        Rigidbody[] _rigidBodyParts;

        /// <summary>
        /// Enable physics ragdoll for Entity.
        /// </summary>
        public void EnableRagdoll()
        {
            ragdollAnimator.enabled = false;
            ragdollCollider.enabled = false;
            ragdollRigidbody.isKinematic = true;

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
            ragdollAnimator.enabled = true;
            ragdollCollider.enabled = true;
            ragdollRigidbody.isKinematic = false;

            foreach (var hitbox in hitboxController.Hitboxes)
            {
                hitbox.Collider.isTrigger = true;
                hitbox.Rigidbody.isKinematic = true;
            }
        }
    }
}