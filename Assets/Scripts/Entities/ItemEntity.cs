using System;

using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

using UZSG.Systems;
using UZSG.Players;
using UZSG.Interactions;
using UZSG.Items;
using UZSG.Items.Weapons;

namespace UZSG.Entities
{
    /// <summary>
    /// Items that appear in the world (e.g. Interactables, Pickupables, etc.)
    /// </summary>
    public class ItemEntity : Entity, IInteractable
    {
        [SerializeField] Item item;
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
        public string Name => item.Name;
        public string ActionText
        {
            get
            {
                if (item.Type == ItemType.Item ||
                    item.Type == ItemType.Tool || 
                    item.Type == ItemType.Equipment ||
                    item.Type == ItemType.Accessory) return "Pick Up";
                if (item.Type == ItemType.Weapon) return "Equip";
                
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

        protected virtual void Awake()
        {
            model = transform.Find("box").gameObject;
            _originalLayer = model.layer;
        }

        protected virtual void Start()
        {
            LoadModel();
        }

        void LoadModel()
        {
            if (item == null || item.IsNone)
            {
                Game.Console.LogWarning($"Item in {transform.position} is not set.");
                return;
            }
            if (!item.Data.Model.IsSet())
            {
                Game.Console.LogWarning($"The item {item.Id} has no model to load.");
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
            if (item != null)
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

        public void Interact(PlayerActions actor, InteractArgs args)
        {
            actor.PickUpItem(this);
        }

        /// <summary>
        /// Gets an Item object from the entity.
        /// </summary>
        public Item AsItem()
        {
            if (item == null || item.IsNone)
            {
                return Item.None;
            }
            
            return new(item, item.Count);
        }

        public void OnLookEnter()
        {
            model.layer = LayerMask.NameToLayer("Outline");
        }

        public void OnLookExit()
        {
            model.layer = _originalLayer;
        }

        public void Despawn()
        {
            Game.Tick.OnSecond -= Second;
            Game.Entity.Kill(this);
        }
    }
}