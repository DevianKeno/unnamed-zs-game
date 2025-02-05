using UnityEngine;

using UZSG.Systems;
using UZSG.Interactions;
using UZSG.Attributes;
using UZSG.Saves;
using UZSG.UI;

namespace UZSG.Entities
{
    /// <summary>
    /// Represents non-player Entities which may or may not have Hitboxes.
    /// </summary>
    public partial class NonPlayerCharacter : Entity, IHasHealthBar, IDamageable
    {
        /// <summary>
        /// This entity has a target, whatever they want to do with it.
        /// </summary>
        [SerializeField] protected Entity targetEntity = null;

        public bool IsAlive { get; protected set; }
        public bool HasHitboxes { get; protected set; }

        [field: Header("NPC Components")]
        public Rigidbody Rigidbody { get; protected set; }
        /// <summary>
        /// Implicit health bar. Call Show() to make it visible.
        /// </summary>
        public EntityHealthBar HealthBar { get; protected set; }
        public EntityHitboxController HitboxController { get; protected set; }
        

        #region Initializing methods

        protected virtual void Awake()
        {
            Rigidbody = GetComponent<Rigidbody>();
            HealthBar = GetComponentInChildren<EntityHealthBar>();
            HitboxController = GetComponent<EntityHitboxController>();

            /// Store in arrays and variables all the NPC's collider, rigid body, and animator.
            /// Get components of ragdoll
            // _ragdollColliders = ragdollBody.GetComponentsInChildren<Collider>();
            // _rigidBodyParts = ragdollBody.GetComponentsInChildren<Rigidbody>();
            // ragdollCollider = GetComponent<BoxCollider>();
            // ragdollAnimator = GetComponentInChildren<Animator>();
            // ragdollRigidbody = GetComponent<Rigidbody>();
        }

        public override void OnSpawn()
        {
            // HealthBar.Initialize();
            InitializeHitboxes();
            IsAlive = true;
            if (IsAlive)
            {
                // DisableRagdoll();
            }
            else
            {
                // EnableRagdoll();
            }
        }

        protected virtual void InitializeHitboxes()
        {
            if (HitboxController != null)
            {
                HasHitboxes = true;
                HitboxController.ReinitializeHitboxes();

                foreach (var hitbox in HitboxController.Hitboxes)
                {
                    hitbox.OnCollision += OnHitboxCollision;
                }
            }
        }

        #endregion
        

        /// <summary>
        /// Called whenever a Hitbox of this character is hit by anything that is an ICollisionSource.
        /// </summary>
        protected virtual void OnHitboxCollision(object sender, HitboxCollisionInfo info) { }
        public virtual void TakeDamage(DamageInfo info) { }
        public virtual void Kill() { }


        #region Subject to be moved to another script
        
        public void SpawnDamageText(Vector3 position)
        {
            Game.Entity.Spawn<DamageText>("damage_text", position);
        }

        public void SpawnBlood(Vector3 position)
        {
            Game.Particles.Create("blood_splat", position);
        }

        #endregion
    }
}