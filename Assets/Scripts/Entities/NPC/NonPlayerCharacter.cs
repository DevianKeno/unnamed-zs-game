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
        /// <summary>
        /// This entity has a target, whatever they want to do with it.
        /// </summary>
        [SerializeField] protected Entity targetEntity = null;
        public bool IsAlive { get; protected set;}
        public bool IsDead { get; protected set;}
        public bool HasHitboxes = false;

        [Header("NPC Components")]
        protected EntityHitboxController hitboxController;
        protected Rigidbody rb;
        public Rigidbody Rigidbody => rb;


        #region Initializing methods

        void Awake()
        {
            rb = GetComponent<Rigidbody>();
        }

        public override void OnSpawn()
        {
            base.OnSpawn();

            InitializeHitboxes();
            LoadDefaultSaveData<EntitySaveData>();
            ReadSaveData(saveData);
            GetRagdollParts();
            IsAlive = true;
        }

        void InitializeHitboxes()
        {
            hitboxController = GetComponent<EntityHitboxController>();
            if (hitboxController != null)
            {
                HasHitboxes = true;
                hitboxController.ReinitializeHitboxes();
                foreach (var hitbox in hitboxController.Hitboxes)
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


        #region Public methods
        
        public virtual void TakeDamage(float value)
        {
            if (Attributes.TryGet("health", out var health))
            {
                health.Remove(value);
                Debug.Log("Has Taken Damage. Current health: " + health.ToString());
                if (health.Value <= 0)
                {
                    IsDead = true;
                }
            }
        }

        #endregion


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