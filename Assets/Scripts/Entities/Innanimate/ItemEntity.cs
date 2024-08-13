using System;

using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

using UZSG.Systems;
using UZSG.Data;
using UZSG.Interactions;
using UZSG.Players;
using UZSG.Items;

namespace UZSG.Entities
{
    /// <summary>
    /// Items that appear in the world (e.g. Interactables, Pickupables, etc.)
    /// </summary>
    public class ItemEntity : Entity, IInteractable
    {
        [SerializeField] Item item;
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
                item = value;
            }
        }
        public string Name => item.Data.Name;
        public string ActionText
        {
            get
            {
                if (item.Data.Type == ItemType.Item ||
                    item.Data.Type == ItemType.Tool || 
                    item.Data.Type == ItemType.Equipment ||
                    item.Data.Type == ItemType.Accessory) return "Pick Up";
                if (item.Data.Type == ItemType.Weapon) return "Equip";
                
                return "Interact with";
            }
        }

        public event EventHandler<InteractArgs> OnInteract;
        /// <summary>
        /// Despawn time in seconds.
        /// </summary>
        public float Age = 3600;

        int _originalLayer;
        GameObject model;
        Rigidbody rb;
        public Rigidbody Rigidbody => rb;

        protected virtual void Awake()
        {
            model = transform.Find("box").gameObject;
            rb = GetComponentInChildren<Rigidbody>();
            _originalLayer = model.layer;
        }

        protected virtual void Start()
        {
            LoadModel();
        }

        void LoadModel()
        {
            if (item.IsNone)
            {
                Game.Console.LogWarning($"Item in {transform.position} is not set.");
                return;
            }
            if (!item.Data.Model.IsSet())
            {
                Game.Console.LogWarning($"The item {item.Data.Id} has no model to load.");
                return;
            }

            Addressables.LoadAssetAsync<GameObject>(item.Data.Model).Completed += (a) =>
            {
                if (a.Status == AsyncOperationStatus.Succeeded)
                {
                    Destroy(model);
                    model = Instantiate(a.Result, transform);
                    model.ChangeTag("Interactable");
                }
            };            
        }

        public override void OnSpawn()
        {
            if (!item.IsNone)
            {
                LoadModel();
            }
            Game.Tick.OnSecond += Second;
        }

        void Second(SecondInfo e)
        {
            Age -= 1;
            if (Age < 0)
            {
                Despawn();
            }
        }

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
    }
}