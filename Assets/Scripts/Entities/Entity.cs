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
        bool _hasAlreadySpawned = false;
        
        [SerializeField] protected EntityData entityData;
        public EntityData EntityData => entityData;
        /// <summary>
        /// Shorthand to get EntityData Id.
        /// </summary>
        public string Id => entityData.Id;
        protected EntitySaveData saveData;

        [SerializeField] protected AttributeCollection attributes;
        public AttributeCollection Attributes => attributes;

        /// <summary>
        /// The transform position of this Entity. 
        /// </summary>
        public Vector3 Position
        {
            get { return transform.position; }
            set { transform.position = value; }
        }
        /// <summary>
        /// The transform rotation of this Entity. 
        /// </summary>
        public Quaternion Rotation
        {
            get { return transform.rotation; }
            set { transform.rotation = value; }
        }


        #region Initializing methods

        protected virtual void Start()
        {
            OnSpawnInternal();
        }

        internal void OnSpawnInternal()
        {
            if (_hasAlreadySpawned) return;
            _hasAlreadySpawned = true;

            OnSpawn();
        }

        protected virtual void LoadDefaultSaveData<T>() where T : EntitySaveData
        {
            saveData = entityData.GetDefaultSaveData<T>();
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
        
        void ReadTransformSaveData(TransformSaveData data)
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


        #region Public methods

        public virtual void Kill(bool notify = true)
        {
            if (notify) Game.Entity.Kill(this);
        }

        #endregion
    }
}