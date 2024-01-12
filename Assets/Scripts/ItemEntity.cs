using UZSG.Player;
using UZSG.Interactions;
using UZSG.Items;
using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UZSG.Systems;

namespace UZSG.Entities
{
    /// <summary>
    /// Items that appear in the world (e.g. Interactables, Pickupables, etc.)
    /// </summary>
    public class ItemEntity : Entity, IInteractable
    {
        public ItemData ItemData;
        public int ItemCount;
        public string Name => ItemData.Name;
        public string Action
        {
            get
            {
                if (ItemData.Type == ItemType.Item ||
                    ItemData.Type == ItemType.Tool || 
                    ItemData.Type == ItemType.Equipment ||
                    ItemData.Type == ItemType.Accessory) return "Pick Up";
                if (ItemData.Type == ItemType.Weapon) return "Equip";
                return "Interact with";
            }
        }

        public event EventHandler<InteractArgs> OnInteract;
        /// <summary>
        /// Despawn time in seconds.
        /// </summary>
        public float Age = 3600;

        GameObject model;
        MeshFilter meshFilter;
        MeshRenderer meshRenderer;
        MeshCollider meshCollider;

        void Awake()
        {
            meshFilter = GetComponent<MeshFilter>();
            meshRenderer = GetComponent<MeshRenderer>();
            meshCollider = GetComponent<MeshCollider>();
        }

        void LoadModel()
        {
            if (ItemData == null)
            {
                Game.Console.LogWarning($"This item has no Item Data, there will be no model to load.");
                return;
            };

            Addressables.LoadAssetAsync<GameObject>(ItemData.AssetReference).Completed += (a) =>
            {
                if (a.Status == AsyncOperationStatus.Succeeded)
                {
                    GameObject obj = a.Result;

                    // obj.transform.SetParent(transform);
                    transform.localScale = obj.transform.localScale;
                    meshFilter.sharedMesh = obj.GetComponent<MeshFilter>().sharedMesh;
                    meshRenderer.sharedMaterial = obj.GetComponent<MeshRenderer>().sharedMaterial;
                    meshCollider.sharedMesh = meshFilter.sharedMesh;
                } else
                {
                    Game.Console.LogWarning($"There is no model for this item");
                }
            };            
        }

        public override void OnSpawn()
        {
            if (ItemData != null) LoadModel();
            Game.Tick.OnSecond += Second;
        }

        void Second(object sender, SecondEventArgs e)
        {
            Age -= 1;
            if (Age < 0)
            {
                Game.Entity.Kill(this);
            }
        }

        /// <summary>
        /// Gets an Item object from the entity.
        /// </summary>
        public Item AsItem()
        {
            Item item = new(ItemData, ItemCount);

            if (ItemData is WeaponData data)
            {
                item = new Weapon(data, 1);
            }
            return item;
        }

        public ItemData GetItemData()
        {
            return ItemData;
        }
        
        public void SetItemData(ItemData data)
        {
            ItemData = data;
        }

        public void SetItemData(string id)
        {
            if (Game.Items.TryGetItemData(id, out ItemData itemData))
            {
                ItemData = itemData;
            } else
            {
                Game.Console.LogDebug($"Unable to set Item Data. There is no item with an id of {id}.");
            }
        }

        /// <summary>
        /// Gets a Weapon object from the entity.
        /// </summary>
        // public Weapon AsWeapon()
        // {
        //     // return new Weapon();
        // }

        public void Interact(PlayerActions actor, InteractArgs args)
        {
            actor.PickUpItem(this);
        }
    }
}