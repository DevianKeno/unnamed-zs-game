using System;

using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

using UZSG.Systems;
using UZSG.Data;
using UZSG.Attributes;
using UZSG.UI.Objects;
using UZSG.Interactions;
using UZSG.Saves;
using UZSG.Entities;
using UZSG.Items;
using UZSG.Players;

namespace UZSG.Objects
{
    public abstract class BaseObject : MonoBehaviour, IAttributable, IPlaceable, IPickupable, ICollisionTarget
    {
        [SerializeField] protected ObjectData objectData;
        public ObjectData ObjectData => objectData;

        [SerializeField] protected AttributeCollection attributes;
        public AttributeCollection Attributes => attributes;
        
        [SerializeField] protected Animator animator;
        public Animator Animator => animator;

        public virtual bool IsPlaced { get; protected set; } = false;
        public virtual bool IsDamageable { get; protected set; } = true;
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
        public event Action<BaseObject> OnDestroyed;
        
        Material material;

        #region Initializing methods

        protected virtual void Start() { }

        internal void PlaceInternal()
        {
            transform.SetParent(Game.World.CurrentWorld.objectsContainer, worldPositionStays: true);
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

            OnPlace();
            this.IsPlaced = true;
        }

        public virtual void Pickup(IInteractActor actor)
        {
            if (actor is Player player)
            {
                if (this.CanBePickedUp && player.Actions.PickUpItem(this.AsItem()))
                {
                    Destruct();
                }
            }
        }

        protected virtual void LoadGUIAsset(AssetReference guiAsset, Action<ObjectGUI> onLoadCompleted = null)
        {
            if (!guiAsset.IsSet())
            {
                Game.Console.LogWithUnity($"There's no GUI set for Workstation '{objectData.Id}'. This won't be usable unless otherwise you set its GUI.");
                return;
            }

            Addressables.LoadAssetAsync<GameObject>(guiAsset).Completed += (a) =>
            {
                if (a.Status == AsyncOperationStatus.Succeeded)
                {
                    var go = Instantiate(a.Result, Vector3.zero, Quaternion.identity, transform);
                    
                    if (go.TryGetComponent<ObjectGUI>(out var gui))
                    {
                        onLoadCompleted?.Invoke(gui);
                        return;
                    }
                }
            };
        }

        public void ReadSaveData(ObjectSaveData saveData)
        {
            InitializeTransform(saveData.Transform);
        }

        public virtual ObjectSaveData WriteSaveData()
        {
            var saveData = new ObjectSaveData()
            {
                InstanceId = GetInstanceID(),
                Id = objectData.Id,
                Transform = new()
                {
                    Position = Utils.ToFloatArray(transform.position),
                    Rotation = Utils.ToFloatArray(transform.rotation.eulerAngles),
                    LocalScale = Utils.ToFloatArray(transform.localScale),
                }
            };

            return saveData;
        }

        #endregion


        #region Public methods

        public virtual Item AsItem()
        {
            if (Game.Items.TryGetData(objectData.Id, out var itemData))
            {
                return new Item(itemData, 1);
            }
            else
            {
                Game.Console.LogWarn($"BaseObject/AsItem() Object '{objectData.Id}' does not have an Item counterpart.");
                return Item.None;
            }
        }

        /// <summary>
        /// <i>Destroys</i> this object in accordance with UZSG laws.
        /// <c>Destruct</c> because <c>Destroy</c> is reserved for UnityEngine's method.
        /// </summary>
        public virtual void Destruct(bool notify = true)
        {
            OnDestroy();
            if (notify) OnDestroyed?.Invoke(this);
            MonoBehaviour.Destroy(gameObject);
        }
        public virtual void HitBy(HitboxCollisionInfo info)
        {
            Game.Particles.Create<MaterialBreak>(Game.Particles.GetParticleData("material_break"), info.ContactPoint, onSpawn: (particle) =>
            {
                particle.SetMaterial(this.material);
            });

            if (IsDamageable)
            {

            }
        }

        protected virtual void OnPlace() { }
        protected virtual void OnDestroy() { }

        #endregion


        void InitializeTransform(TransformSaveData data)
        {
            var position = Utils.FromFloatArray(data.Position);
            var rotation = Utils.FromFloatArray(data.Rotation);
            // var scale = Utils.FromNumericVec3(data.LocalScale);
            transform.SetPositionAndRotation(position, Quaternion.Euler(rotation));
            // transform.localScale = scale;
        }
    }
}