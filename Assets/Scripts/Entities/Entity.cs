using System;

using UnityEngine;

using UZSG.Systems;
using UZSG.Data;
using UZSG.Interactions;
using UZSG.Saves;

namespace UZSG.Entities
{
    /// <summary>
    /// Represent dynamic objects that appear in the World.
    /// </summary>
    public abstract class Entity : MonoBehaviour, ISaveDataReadWrite<EntitySaveData>
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

        public bool HasHitboxes = false;
        protected EntityHitboxController hitboxes;

        bool _isAlive;
        public bool IsAlive => _isAlive;


        #region Initializing methods

        void Awake()
        {
            hitboxes = GetComponent<EntityHitboxController>();
        }

        internal void OnSpawnInternal()
        {
            OnSpawn();
            if (HasHitboxes) InitializeHitboxEvents();
        }

        /// <summary>
        /// Called after the EntityManager spawned callback.
        /// You can modify the entity's attributes before this calls.
        /// </summary>
        public virtual void OnSpawn()
        {
        }

        public void ReadSaveJson(EntitySaveData saveData)
        {
            
        }

        public EntitySaveData WriteSaveJson()
        {
            var saveData = new EntitySaveData()
            {
                Transform = new()
                {
                    Position = transform.position,
                    Rotation = transform.rotation,
                    LocalScale = transform.localScale,
                }
            };

            return saveData;
        }

        #endregion
        

        public virtual void Kill()
        {
            Game.Entity.Kill(this);
        }
        
        protected void InitializeHitboxEvents()
        {
            foreach (var hitbox in hitboxes.Hitboxes)
            {
                hitbox.OnHit += OnHitboxCollide;
            }
        }
        
        protected virtual void OnHitboxCollide(object sender, HitboxCollisionInfo info) { }
    }
}