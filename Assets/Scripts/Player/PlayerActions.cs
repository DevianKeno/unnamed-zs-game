using UnityEngine;
using UnityEngine.InputSystem;
using UZSG.Systems;
using UZSG.Items;
using UZSG.Interactions;
using UZSG.Entities;
using UZSG.FPP;

namespace UZSG.Player
{
    public enum PlayerStates { Idle, Run, Jump, Walk, Crouch }

    /// <summary>
    /// Represents the different actions the Player can do.
    /// </summary>
    [RequireComponent(typeof(PlayerCore))]
    public class PlayerActions : MonoBehaviour
    {
        PlayerCore _player;
        public PlayerCore Player { get => _player; }

        FPPHandler _fpp;

        void Awake()
        {
            _player = GetComponent<PlayerCore>();
            _fpp = GetComponent<FPPHandler>();
        }

        void Start()
        {
        }

        public void Jump()
        {            
        }

        public void SelectHotbar(int index)
        {
            if (index < 0 || index > 9) return;

            // OnActionPerform?.Invoke(this, new()
            // {
            //     Action = Actions.SelectHotbar
            // });

            // if (index == 1) _fpp.Equip();

            Debug.Log($"Equipped hotbar slot {index}");
        }

        public void PerformPrimary()
        {
            // Attack
        }

        public void PerformSecondary()
        {
            
        }

        public void Run(InputAction.CallbackContext context)
        {
            
        }

        public void Crouch(InputAction.CallbackContext context)
        {
            
        }

        /// <summary>
        /// Make this player interact with an Interactable object.
        /// </summary>
        public void Interact(IInteractable obj)
        {
            obj.Interact(this, new InteractArgs());
        }

        public void Equip()
        {

        }

        /// <summary>
        /// Pick up item from ItemEntity and put in the inventory.
        /// There is currently no checking if inventory is full.
        /// </summary>
        public void PickUpItem(ItemEntity itemEntity)
        {
            if (!_player.CanPickUpItems) return;
            if (_player.Inventory == null) return;

            Item item = itemEntity.AsItem();
            
            if (item.Type == ItemType.Weapon)
            {
                if (_player.Inventory.Hotbar.Mainhand.IsEmpty)
                {
                    _player.Inventory.Hotbar.Mainhand.PutItem(item);
                    Destroy(itemEntity.gameObject);

                    // Weapon weapon = new(_player.Inventory.Hotbar.Mainhand.Item);
                    // _FPPHandler.Load(weapon);

                }
            } else if (item.Type == ItemType.Tool)
            {
                if (_player.Inventory.Hotbar.Offhand.IsEmpty)
                {
                    _player.Inventory.Hotbar.Offhand.PutItem(item);
                    Destroy(itemEntity.gameObject);
                } // else try to put in other hotbar slots (3-0) only if available
            } else
            {
                if (_player.Inventory.Bag.TryPutNearest(item))
                {
                    Destroy(itemEntity.gameObject);
                }
            }            
        }

        /// <summary>
        /// I want the cam to lock and cursor to appear only when the key is released :P
        /// </summary>
        public void ToggleInventory()
        {
            Game.UI.InventoryUI.ToggleVisibility();
        }
    }
}
