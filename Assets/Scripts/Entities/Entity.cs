using System;

using UnityEngine;


using UZSG.Data;
using UZSG.Saves;
using UZSG.Attributes;

namespace UZSG.Entities
{
    /// <summary>
    /// Base class for dynamic objects that appear in the World.
    /// </summary>
    public abstract class Entity : MonoBehaviour, IAttributable, ISaveDataReadWrite<EntitySaveData>
    {
        [SerializeField] int instanceId; /// so we can see In inspector
        public int InstanceId => instanceId;
        [Space]
        
        [SerializeField] protected EntityData entityData;
        public EntityData EntityData => entityData;
        /// <summary>
        /// Shorthand to get EntityData Id.
        /// </summary>
        public virtual string Id => entityData.Id;
        protected EntitySaveData saveData;
        bool _hasAlreadySpawned = false;

        [SerializeField] protected AttributeCollection attributes;
        public virtual AttributeCollection Attributes => attributes;

        /// <summary>
        /// The transform position of this Entity in world space.
        /// </summary>
        public virtual Vector3 Position
        {
            get { return transform.position; }
            set { transform.position = value; }
        }
        /// <summary>
        /// The transform rotation of this Entity. 
        /// </summary>
        public virtual Quaternion Rotation
        {
            get { return transform.rotation; }
            set { transform.rotation = value; }
        }


        #region Entity events

        public event Action<Entity> OnDespawned;

        #endregion


        #region Initializing methods

        protected virtual void Start()
        {
            OnSpawnInternal();
        }

        internal void OnSpawnInternal()
        {
            if (_hasAlreadySpawned) return;

            try /// throws an error for networked objects
            {            
                transform.SetParent(Game.World.CurrentWorld.entitiesContainer, worldPositionStays: true);
            }
            catch { }
            _hasAlreadySpawned = true;
            instanceId = GetInstanceID();
            OnSpawn();
        }

        /// <summary>
        /// Immediately assigns to current save data.
        /// </summary>
        protected virtual void LoadDefaultSaveData<T>() where T : EntitySaveData
        {
            this.saveData = new();
            this.saveData = entityData.GetDefaultSaveData<T>();
        }

        /// <summary>
        /// Called after the EntityManager spawned callback.
        /// You can modify the entity's attributes before this calls.
        /// </summary>
        public virtual void OnSpawn() { }
        protected virtual void OnDespawn() { }

        #endregion
        

        public virtual void ReadSaveData(EntitySaveData saveData)
        {
            this.saveData = saveData;
            
            if (saveData.Transform != null)
            {
                ReadTransformSaveData(saveData.Transform);
            }
            /// Read attributes()
            attributes = new();
            attributes.ReadSaveData(saveData.Attributes);

            /// Read etc.
        }
        
        public virtual EntitySaveData WriteSaveData()
        {
            var saveData = new EntitySaveData()
            {
                InstanceId = GetInstanceID(),
                Id = entityData.Id,
                Transform = new()
                {
                    Position = Utils.ToFloatArray(transform.position),
                    Rotation = Utils.ToFloatArray(transform.rotation.eulerAngles),
                    LocalScale = Utils.ToFloatArray(transform.localScale),
                }
            };

            return saveData;
        }
        
        protected virtual void ReadTransformSaveData(TransformSaveData data)
        {
            var position = Utils.FromFloatArray(data.Position);
            var rotation = Utils.FromFloatArray(data.Rotation);
            // var scale = Utils.FromNumericVec3(data.LocalScale);
            transform.SetPositionAndRotation(position, Quaternion.Euler(rotation));
            // transform.localScale = scale;
        }


        #region Public methods

        /// <summary>
        /// Kill this entity.
        /// </summary>
        public void Despawn(bool notify = true)
        {
            OnDespawn();
            if (notify) OnDespawned?.Invoke(this);
            MonoBehaviour.Destroy(gameObject);
        }

        #endregion

        /// <summary>
        /// Check whether this entity is within the radius with the given position as center.
        /// </summary>
        public virtual bool InRangeOf(Vector3 center, float radius)
        {
            var distance = Vector3.Distance(transform.position, center);
            return distance <= radius; 
        }

        /// <summary>
        /// Check whether this Entity can "see" the given Entity.
        /// </summary>
        public virtual bool CanSee(Entity etty)
        {
            Vector3 direction = (etty.Position - this.Position).normalized;
            float distance = Vector3.Distance(this.Position, etty.Position);

            /// TODO: the `this.Position` below should be EyeLevel
            if (Physics.Raycast(this.Position, direction, out var hit, distance))
            {
                if (hit.collider.TryGetComponent<Entity>(out var detected) &&
                    detected == etty) /// check if same entity
                {
                    return true;
                }
            }

            return false;
        }
    }
}