using System;

using UnityEngine;

using UZSG.Systems;
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
        public string Id => entityData.Id;
        protected EntitySaveData saveData;
        bool _hasAlreadySpawned = false;

        [SerializeField] protected AttributeCollection attributes;
        public AttributeCollection Attributes => attributes;

        /// <summary>
        /// The transform position of this Entity. 
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

        public event Action<Entity> OnKilled;

        #endregion


        #region Initializing methods

        protected virtual void Start()
        {
            OnSpawnInternal();
        }

        internal void OnSpawnInternal()
        {
            if (_hasAlreadySpawned) return;

            _hasAlreadySpawned = true;
            instanceId = GetInstanceID();
            OnSpawn();
        }

        protected virtual void LoadDefaultSaveData<T>() where T : EntitySaveData
        {
            this.saveData = entityData.GetDefaultSaveData<T>();
        }

        /// <summary>
        /// Called after the EntityManager spawned callback.
        /// You can modify the entity's attributes before this calls.
        /// </summary>
        public virtual void OnSpawn() { }

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
                    Position = Utils.FromUnityVec3(transform.position),
                    Rotation = Utils.FromUnityVec3(transform.rotation.eulerAngles),
                    LocalScale = Utils.FromUnityVec3(transform.localScale),
                }
            };

            return saveData;
        }
        
        protected virtual void ReadTransformSaveData(TransformSaveData data)
        {
            var position = Utils.FromNumericVec3(data.Position);
            var rotation = Utils.FromNumericVec3(data.Rotation);
            // var scale = Utils.FromNumericVec3(data.LocalScale);
            transform.SetPositionAndRotation(position, Quaternion.Euler(rotation));
            // transform.localScale = scale;
        }


        #region Public methods

        /// <summary>
        /// Kill this entity.
        /// </summary>
        public void Kill(bool notify = true)
        {
            OnKill();
            if (notify) OnKilled?.Invoke(this);
            MonoBehaviour.Destroy(gameObject);
        }

        #endregion

        protected virtual void OnKill() { }
    }
}