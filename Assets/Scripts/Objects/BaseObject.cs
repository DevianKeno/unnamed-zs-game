using System;

using UnityEngine;

using UZSG.Attributes;
using UZSG.Data;
using UZSG.Interactions;
using UZSG.Items;
using UZSG.Saves;
using UZSG.Worlds;

namespace UZSG.Objects
{
    public abstract class BaseObject : MonoBehaviour, IAttributable, IPlaceable, ICollisionTarget, ISaveDataReadWrite<BaseObjectSaveData>
    {
        [SerializeField] protected ObjectData objectData;
        public ObjectData ObjectData => objectData;
        
        public string DisplayName => objectData.DisplayNameTranslatable;

        [SerializeField] protected AttributeCollection attributes;
        public AttributeCollection Attributes => attributes;
        
        [SerializeField] protected Animator animator;
        public Animator Animator => animator;

        protected Chunk chunk;

        public virtual bool IsPlaced { get; protected set; } = false;
        public virtual bool IsDamageable { get; protected set; } = true;
        public virtual bool IsDamaged
        {
            get
            {
                if (Attributes.TryGet(AttributeId.Health, out var health)) return !health.IsFull;
                return false;
            }
        }
        public virtual bool IsDirty { get; protected set; } = false;
        public virtual bool CanBePickedUp
        {
            get
            {
                if (this.attributes.TryGet("health", out var health))
                {
                    return health.IsFull && objectData.CanBePickedUp;
                }
                return objectData.CanBePickedUp;
            }
        }
        /// <summary>
        /// The transform position of this Object. 
        /// </summary>
        public virtual Vector3 Position
        {
            get { return transform.position; }
            set { transform.position = value; }
        }
        /// <summary>
        /// The transform rotation of this Object. 
        /// </summary>
        public virtual Quaternion Rotation
        {
            get { return transform.rotation; }
            set { transform.rotation = value; }
        }
        /// <summary>
        /// The local transform rotation of this Object. 
        /// </summary>
        public virtual Quaternion LocalRotation
        {
            get { return transform.localRotation; }
            set { transform.localRotation = value; }
        }
        
        public event EventHandler<HitboxCollisionInfo> OnCollision;
        public event Action<BaseObject> OnDestructed;
        
        protected Material material;


        #region Initializing methods

        internal void PlaceInternal()
        {
            if (this is IInteractable)
            {
                foreach (var coll in GetComponentsInChildren<Collider>())
                {
                    coll.gameObject.tag = Tags.INTERACTABLE;
                }
            }
            var renderer = GetComponentInChildren<Renderer>();
            if (renderer != null)
            {
                this.material = renderer.material;
            }

            if (objectData.HasAudio)
            {
                Game.Audio.LoadAudioAssets(objectData.AudioAssetsData);
            }

            chunk = Game.World.CurrentWorld.GetChunkBy(worldPosition: this.Position);
            if (chunk != null)
            {
                transform.SetParent(chunk.transform, worldPositionStays: true);
            
                if (this is INaturallyPlaced)
                {
                }
                else
                {
                    chunk.RegisterObject(this);
                }
            }

            OnPlaceEvent();
            this.IsPlaced = true;
        }

        internal void DestructInternal()
        {
            if (chunk != null)
            {
                chunk = Game.World.CurrentWorld.GetChunkBy(worldPosition: this.Position);
            }
            chunk?.MarkDirty();
        }

        #endregion


        #region Public methods
        
        public virtual void ReadSaveData(BaseObjectSaveData saveData)
        {
            ReadTransform(saveData.Transform);
        }

        public virtual BaseObjectSaveData WriteSaveData()
        {
            var saveData = new BaseObjectSaveData()
            {
                Id = objectData.Id,
                Transform = WriteTransform(),
            };

            return saveData;
        }
        /// <summary>
        /// Returns the Item representation of this object (if any).
        /// Returns <c>Item.None</c> if not represented.
        /// </summary>
        /// <returns></returns>
        public virtual Item AsItem()
        {
            if (Game.Items.TryGetData(objectData.Id, out var itemData))
            {
                return new Item(itemData, 1);
            }
            else
            {
                Game.Console.LogDebug($"[BaseObject/AsItem()]: Object '{objectData.Id}' does not have an Item counterpart.");
                return Item.None;
            }
        }

        public void MarkDirty()
        {
            this.IsDirty = true;
            if (chunk != null)
            {
                chunk.MarkDirty();
            }
        }

        /// <summary>
        /// <i>Destroys</i> this object in accordance with UZSG laws.
        /// <c>Destruct</c> because <c>Destroy</c> is reserved for UnityEngine's method.
        /// </summary>
        public void Destruct()
        {
            DestructInternal();
            OnDestructEvent();
            OnDestructed?.Invoke(this);
            MonoBehaviour.Destroy(gameObject);
        }

        public virtual void HitBy(HitboxCollisionInfo info)
        {
            Game.Particles.Create<MaterialBreak>(Game.Particles.GetParticleData("material_break"), info.ContactPoint, onSpawn: (particle) =>
            {
                particle.transform.rotation = Quaternion.LookRotation(Vector3.Reflect(info.Velocity, info.ContactNormal));
                particle.SetMaterial(this.material);
            });

            if (IsDamageable)
            {
                //TODO:
            }
        }

        /// <summary>
        /// Raised once when this object is placed/built.
        /// </summary>
        protected virtual void OnPlaceEvent() { }
        /// <summary>
        /// Raised once when this object is destructed (broken, pick up, taken away from the world, etc.).
        /// </summary>
        protected virtual void OnDestructEvent() { }

        #endregion


        public void ReadTransform(TransformSaveData data)
        {
            var position = Utils.FromFloatArray(data.Position);
            var rotation = Utils.FromFloatArray(data.Rotation);
            // var scale = Utils.FromNumericVec3(data.LocalScale);
            transform.SetPositionAndRotation(position, Quaternion.Euler(rotation));
            // transform.localScale = scale;
        }
        
        public TransformSaveData WriteTransform()
        {
            return new()
            {
                Position = Utils.ToFloatArray(transform.position),
                Rotation = Utils.ToFloatArray(transform.rotation.eulerAngles),
                // LocalScale = Utils.ToFloatArray(transform.localScale),
            };
        }
    }
}