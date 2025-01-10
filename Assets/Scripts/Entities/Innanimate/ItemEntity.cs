using System;

using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

using UZSG.Systems;
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
    public class ItemEntity : Entity, ILookable, IInteractable, IWorldCleanupable
    {
        public const int DespawnTimeSeconds = 60;

        [SerializeField] Item item = Item.None;
        /// <summary>
        /// Get the actual 'Item' from the Item Entity.
        /// </summary>
        public Item Item
        {
            get => AsItem();
            set => item = new(value);
        }
        public string Name
        {
            get
            {
                if (item.Data.Type == ItemType.Item)
                {
                    return $"{item.Count} {item.Data.DisplayName}";
                }
                else
                {
                    return $"{item.Data.DisplayName}";
                }
            }
        }
        public string Action
        {
            get
            {
                return item.Data.Type switch
                {
                    ItemType.Item or
                    ItemType.Tool or
                    ItemType.Equipment or
                    ItemType.Tile or
                    ItemType.Accessory => "Pick Up",
                    ItemType.Weapon => "Equip",
                    _ => "Interact with",
                };
            }
        }
        public LookableType LookableType => LookableType.Interactable; 
        public bool AllowInteractions { get; set; } = true;
        /// <summary>
        /// Despawn time in seconds.
        /// </summary>
        public int Age;
        public event EventHandler<IInteractArgs> OnInteract;

        int _originalLayer;
        bool _isModelLoaded;

        GameObject model;
        Rigidbody rb;
        public Rigidbody Rigidbody => rb;


        #region Initializing methods

        protected virtual void Awake()
        {
            rb = GetComponentInChildren<Rigidbody>();
            model = transform.Find("box").gameObject;
            _originalLayer = model.layer;
        }

        protected override void Start()
        {
            LoadModel();
            Age = DespawnTimeSeconds;
        }

        public override void OnSpawn()
        {
            base.OnSpawn();

            LoadModel();
            Age = DespawnTimeSeconds;
            Game.Tick.OnSecond += Second;
        }

        void LoadModel()
        {
            if (_isModelLoaded) return;

            if (item.IsNone)
            {
                Game.Console.Warn($"Item in {transform.position} is a None Item.");
                return;
            }
            if (!item.Data.Model.IsSet())
            {
                Game.Console.Warn($"The model asset of Item {item.Data.Id} is missing or not set.");
                return;
            }

            Addressables.LoadAssetAsync<GameObject>(item.Data.Model).Completed += (a) =>
            {
                if (a.Status == AsyncOperationStatus.Succeeded)
                {
                    Destroy(model);
                    model = Instantiate(a.Result, transform);
                    /// Change the tag of the actual Item model
                    /// so it becomes "interactable"
                    model.ChangeTag("Interactable");
                    _isModelLoaded = true;
                }
            };
        }

        void Second(SecondInfo e)
        {
            Age -= 1;
            if (Age < 0)
            {
                Kill();
            }
        }

        public override void ReadSaveData(EntitySaveData saveData)
        {
            if (saveData is not ItemEntitySaveData sd) return;

            base.ReadSaveData(saveData);
            if (sd.Item.Id == "none") Destroy(gameObject);

            item = new Item(sd.Item.Id, sd.Item.Count);
            if (sd.Item.HasAttributes)
            {
                item.Attributes.ReadSaveData(sd.Item.Attributes);
            }
        }
        
        public override EntitySaveData WriteSaveData()
        {
            var sd = base.WriteSaveData();
            
            var saveData = new ItemEntitySaveData()
            {
                InstanceId = sd.InstanceId,
                Id = entityData.Id,
                Transform = sd.Transform,
                Item = item.WriteSaveData(),
                Age = Age
            };

            return saveData;
        }

        #endregion


        protected override void OnKill()
        {
            _isModelLoaded = false;
            Game.Tick.OnSecond -= Second;
        }


        #region Public methods

        public void Interact(IInteractActor actor, IInteractArgs args)
        {
            if (actor is not Player player) return;

            if (player.Actions.PickUpItem(this))
            {
                Kill();
            }
        }

        /// <summary>
        /// Gets an Item object from the entity.
        /// </summary>
        public Item AsItem()
        {
            if (item.IsNone)
            {
                return Item.None;
            }
            else
            {
                return new(item);
            }
        }

        /// <summary>
        /// Apply a throw force to this Item Entity.
        /// </summary>
        public void Throw(Vector3 direction, float power)
        {
            Rigidbody.AddForce(direction * power, ForceMode.Impulse);
        }

        bool _isAlreadyBeingLookedAt = false;

        public void OnLookEnter()
        {
            if (_isAlreadyBeingLookedAt) return;
            _isAlreadyBeingLookedAt = true;

            if (model != null)
            {
                /// render screen space outlines
                model.layer = LayerMask.NameToLayer("Outline");
            }
        }

        public void OnLookExit()
        {
            _isAlreadyBeingLookedAt = false;
            
            if (model != null)
            {
                model.layer = _originalLayer;
            }
        }

        public void Cleanup()
        {
            if (item.IsNone)
            {
                Kill(notify: false);
            }
        }

        #endregion
    }
}