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
            get
            {
                return item;
            }
            set
            {
                item = new(value);
            }
        }
        public string Name
        {
            get
            {
                if (item.Data.Type == ItemType.Item)
                {
                    return $"{item.Count} {item.Data.Name}";
                }
                else
                {
                    return $"{item.Data.Name}";
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
                    ItemType.Accessory => "Pick Up",
                    ItemType.Weapon => "Equip",
                    _ => "Interact with",
                };
            }
        }
        public LookableType LookableType => LookableType.Interactable; 
        public event EventHandler<InteractArgs> OnInteract;
        /// <summary>
        /// Despawn time in seconds.
        /// </summary>
        public int Age = DespawnTimeSeconds;

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

        protected virtual void Start()
        {
            LoadModel();
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
                Game.Console.LogWarning($"Item in {transform.position} is a None Item.");
                return;
            }
            if (!item.Data.Model.IsSet())
            {
                Game.Console.LogWarning($"The model asset of Item {item.Data.Id} is missing or not set.");
                return;
            }

            Addressables.LoadAssetAsync<GameObject>(item.Data.Model).Completed += (a) =>
            {
                if (a.Status == AsyncOperationStatus.Succeeded)
                {
                    Destroy(model);
                    model = Instantiate(a.Result, transform);
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
                Despawn();
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


        #region Public methods

        public void Interact(IInteractActor actor, InteractArgs args)
        {
            if (actor is not Player player) return;

            player.Actions.PickUpItem(this);
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
            
            return new(item, item.Count);
        }

        public void OnLookEnter()
        {
            if (model != null)
            {
                model.layer = LayerMask.NameToLayer("Outline");
            }
        }

        public void OnLookExit()
        {
            if (model != null)
            {
                model.layer = _originalLayer;
            }
        }

        public void Despawn()
        {
            Game.Tick.OnSecond -= Second;
            Game.Entity.Kill(this);
        }

        public void Cleanup()
        {
            if (item.IsNone)
            {
                Destroy(gameObject);
            }
        }

        #endregion
    }
}