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

            if (hitboxes != null) HasHitboxes=true;
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

        public virtual void ReadSaveData(EntitySaveData saveData)
        {
            InitializeTransform(saveData.Transform);
        }
        
        public virtual EntitySaveData WriteSaveData()
        {
            var saveData = new EntitySaveData()
            {
                InstanceId = GetInstanceID(),
                Id = entityData.Id,
                Transform = new()
                {
                    Position = new System.Numerics.Vector3(
                        transform.position.x,
                        transform.position.y,
                        transform.position.z
                    ),
                    Rotation = new System.Numerics.Quaternion(
                        transform.rotation.x,
                        transform.rotation.y,
                        transform.rotation.z,
                        transform.rotation.w
                    ),
                    LocalScale = new System.Numerics.Vector3(
                        transform.localScale.x,
                        transform.localScale.y,
                        transform.localScale.z
                    )
                }
            };

            return saveData;
        }

        #endregion
        

        public virtual void Kill(bool notify = true)
        {
            if (notify) Game.Entity.Kill(this);
        }
        
        protected void InitializeHitboxEvents()
        {
            foreach (var hitbox in hitboxes.Hitboxes)
            {
                hitbox.OnHit += OnHitboxCollide;
            }
        }
        
        protected virtual void OnHitboxCollide(object sender, HitboxCollisionInfo info) { }
        
        void InitializeTransform(TransformSaveData data)
        {
            var position = new Vector3(
                data.Position.X,
                data.Position.Y,
                data.Position.Z
            );
            var rotation = new Quaternion(
                data.Rotation.X,
                data.Rotation.Y,
                data.Rotation.Z,
                data.Rotation.W
            );
            var scale = new Vector3(
                data.LocalScale.X,
                data.LocalScale.Y,
                data.LocalScale.Z
            );
            transform.SetPositionAndRotation(position, rotation);
            transform.localScale = scale;
        }
    }
}