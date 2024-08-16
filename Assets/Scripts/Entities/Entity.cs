using System;

using UnityEngine;

using UZSG.Systems;
using UZSG.Data;
using UZSG.Interactions;

namespace UZSG.Entities
{
    /// <summary>
    /// Represent dynamic objects that appear in the World.
    /// </summary>
    public abstract class Entity : MonoBehaviour
    {
        protected const string entityDefaultsPath = "/Resources/Defaults/Entities/";

        [SerializeField] protected EntityData entityData;
        public EntityData EntityData => entityData;
        public string Id => entityData.Id;
        public Vector3 Position
        {
            get { return transform.position; }
            set { transform.position = value; }
        }
        [SerializeField] protected AudioSourceController audioSourceController;
        public AudioSourceController AudioSourceController => audioSourceController;
        protected EntityHitboxController hitboxes;

        void Awake()
        {
            hitboxes = GetComponent<EntityHitboxController>();
        }

        internal void OnSpawnInternal()
        {
            OnSpawn();
            InitializeHitboxEvents();
        }

        /// <summary>
        /// Called after the EntityManager spawned callback.
        /// You can modify the entity's attributes before this calls.
        /// </summary>
        public virtual void OnSpawn()
        {
        }

        public virtual void Kill()
        {
            Game.Entity.Kill(this);
        }
        
        protected void InitializeHitboxEvents()
        {
            foreach (var hitbox in hitboxes.Hitboxes)
            {
                hitbox.OnHit += OnCollision;
            }
        }
        
        protected virtual void OnCollision(object sender, CollisionHitInfo info) { }
    }
}