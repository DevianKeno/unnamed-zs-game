using System;
using System.Threading.Tasks;
using System.Collections.Generic;

using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

using UZSG.Data;
using UZSG.Interactions;
using UZSG.Items;
using UZSG.Saves;
using UZSG.Worlds;

namespace UZSG.Entities
{
    /// <summary>
    /// Items that appear in the world (e.g. Interactables, Pickupables, etc.)
    /// </summary>
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    public class ItemEntity : Entity, IInteractable, IWorldCleanupable
    {
        public const int DESPAWN_TIME_SECONDS = 3600;

        [SerializeField] ItemData itemData;
        [SerializeField] Item item;
        public int Count = 0;
        /// <summary>
        /// Get the 'Item' from the Item Entity.
        /// </summary>
        public Item Item
        {
            get => item;
            set
            {
                if (value.Data == null)
                {
                    itemData = null;
                    item = Item.None;
                }
                else
                {
                    itemData = Game.Items.GetData(value.Id);
                    item = value;
                    Count = value.Count;
                }
                LoadModelAsset();
            }
        }
        public string DisplayName
        {
            get
            {
                if (this.Count > 1)
                {
                    return $"{Count} {this.itemData.DisplayNameTranslatable}";
                }
                else
                {
                    return $"{this.itemData.DisplayNameTranslatable}";
                }
            }
        }
        public bool AllowInteractions { get; set; } = true;
        /// <summary>
        /// Despawn time in seconds.
        /// </summary>
        public int Age;

        [Header("Components")]
        [SerializeField] MeshFilter meshFilter;
        [SerializeField] MeshRenderer meshRenderer;
        [SerializeField] Rigidbody rb;
        public Rigidbody Rigidbody => rb;
        [SerializeField] List<Collider> colliders = new();

        [SerializeField] AssetReference cardboardBoxModel;


        #region Event callbacks

#if UNITY_EDITOR
        void OnValidate()
        {
            if (gameObject.activeInHierarchy && !Application.isPlaying)
            {
                this.entityData = Resources.Load<EntityData>("Data/Entities/item");
                LoadModelAsset();
            }
        }
#endif

        public override void OnSpawnEvent()
        {
            base.OnSpawnEvent();
            
            LoadModelAsset();
            Age = DESPAWN_TIME_SECONDS;
            
            Game.Tick.OnSecond -= OnSecond;
            Game.Tick.OnSecond += OnSecond;
        }

        protected override void OnDespawnEvent()
        {
            Game.Tick.OnSecond -= OnSecond;
        }

        void OnSecond(SecondInfo e)
        {
            Age -= 1;
            if (Age < 0)
            {
                Despawn();
            }
        }

        public void OnLookEnter()
        {
            /// render screen space outlines
            gameObject.layer = Game.Entity.OUTLINED_LAYER;
        }

        public void OnLookExit()
        {
            gameObject.layer = Game.Entity.DEFAULT_LAYER;
        }

        #endregion


        void LoadModelAsset()
        {
            AssetReference toLoad;
            if (itemData == null || !itemData.EntityModel.IsSet())
            {
                toLoad = cardboardBoxModel;
            }
            else
            {
                toLoad = this.itemData.EntityModel;
            }
            Addressables.LoadAssetAsync<GameObject>(toLoad).Completed += (a) =>
            {
                if (a.Status == AsyncOperationStatus.Succeeded)
                {
                    InitializeModel(a.Result);
                }
            };
        }

        async void InitializeModel(GameObject asset)
        {
            await Task.Yield();
            if (asset.TryGetComponent(out MeshFilter otherMeshFilter))
            {
                meshFilter = gameObject.GetOrAddComponent<MeshFilter>();
                meshFilter.sharedMesh = otherMeshFilter.sharedMesh;
            }
            if (asset.TryGetComponent(out MeshRenderer otherMeshRenderer))
            {
                meshRenderer = gameObject.GetOrAddComponent<MeshRenderer>();
                meshRenderer.sharedMaterials = otherMeshRenderer.sharedMaterials;
            }
            if (asset.TryGetComponent(out Rigidbody otherRb))
            {
                rb = gameObject.GetOrAddComponent<Rigidbody>();
                rb.mass = otherRb.mass;
                rb.drag = otherRb.drag;
                rb.angularDrag = otherRb.angularDrag;
                rb.useGravity = otherRb.useGravity;
                rb.isKinematic = otherRb.isKinematic;
            }

            foreach (var c in GetComponents<Collider>())
            {
#if UNITY_EDITOR
                DestroyImmediate(c);
#else
                Destroy(c);
#endif
            }
            colliders.Clear();
            foreach (var otherCollider in asset.GetComponents<Collider>())
            {
                var newCollider = CopyCollider(otherCollider);
                colliders.Add(newCollider);
            }
        }

        void DestroyItemComponents()
        {
            Destroy(meshFilter);
            Destroy(meshRenderer);
            Destroy(rb);
            foreach (var c in colliders)
            {
                Destroy(c);
            }
        }

        public override void ReadSaveData(EntitySaveData saveData)
        {
            if (saveData is not ItemEntitySaveData isd) return;

            base.ReadSaveData(saveData);
            if (isd.Item.Id == "none") Despawn();

            var item = new Item(isd.Item.Id, isd.Item.Count);
            this.Item = item;
            // this.Item.ReadSaveData();
            if (isd.Item.HasAttributes)
            {
                /// TODO: unhandled
                this.Item.Attributes.ReadSaveData(isd.Item.Attributes);
            }
        }
        
        public override EntitySaveData WriteSaveData()
        {
            var sd = base.WriteSaveData();
            
            var saveData = new ItemEntitySaveData()
            {
                Id = entityData.Id,
                Transform = sd.Transform,
                Item = this.item.WriteSaveData(),
                Age = Age
            };

            return saveData;
        }

        InteractType GetInteractType()
        {
            return this.itemData.Type switch
            {
                ItemType.Item or
                ItemType.Tool or
                ItemType.Equipment or
                ItemType.Object or
                ItemType.Accessory => InteractType.PickUp,
                ItemType.Weapon => InteractType.Equip,
                _ => InteractType.Interact,
            };
        }

        #region Public methods

        public List<InteractAction> GetInteractActions()
        {
            var actions = new List<InteractAction>();
        
            actions.Add(new()
            {
                Type = GetInteractType(),
                Interactable = this,
                InputAction = Game.Input.InteractPrimary,
            });

            return actions;
        }
        
        public void Interact(InteractionContext context)
        {
            if (context.Actor is not Player player) return;

            if (player.Actions.PickUpItem(this))
            {
                Despawn();
            }
        }

        /// <summary>
        /// Apply a throw force to this Item Entity.
        /// </summary>
        public void Throw(Vector3 direction, float power)
        {
            Rigidbody.AddForce(direction * power, ForceMode.Impulse);
        }

        public void Cleanup()
        {
            if (this.item.IsNone)
            {
                Despawn(notify: false);
            }
        }

        #endregion


        /// <summary>
        /// Copies a Collider component by matching its type.
        /// </summary>
        Collider CopyCollider(Collider original)
        {
            switch (original)
            {
                case BoxCollider box:
                {
                    var newBox = gameObject.AddComponent<BoxCollider>();
                    newBox.center = box.center;
                    newBox.size = box.size;
                    return newBox;
                }
                case SphereCollider sphere:
                {
                    var newSphere = gameObject.AddComponent<SphereCollider>();
                    newSphere.center = sphere.center;
                    newSphere.radius = sphere.radius;
                    return newSphere;
                }
                case CapsuleCollider capsule:
                {
                    var newCapsule = gameObject.AddComponent<CapsuleCollider>();
                    newCapsule.center = capsule.center;
                    newCapsule.radius = capsule.radius;
                    newCapsule.height = capsule.height;
                    newCapsule.direction = capsule.direction;
                    return newCapsule;
                }
                case MeshCollider meshCollider:
                {
                    var newMeshCollider = gameObject.AddComponent<MeshCollider>();
                    newMeshCollider.sharedMesh = meshCollider.sharedMesh;
                    newMeshCollider.convex = meshCollider.convex;
                    return newMeshCollider;
                }
                default:
                {
                    Debug.LogWarning($"Unsupported collider type: {original.GetType()}");
                    return null;
                }
            }
        }
    }
}