using UnityEngine;
using UnityEngine.InputSystem;
using URMG.Systems;
using URMG.Items;
using URMG.Interactions;
using System.Runtime.InteropServices;
using System;
using URMG.Inventory;

namespace URMG.Player
{

    /// <summary>
    /// Represents the different actions the Player can do.
    /// </summary>
    [RequireComponent(typeof(PlayerCore))]
    public class PlayerActions : MonoBehaviour
    {
        public enum Actions { Jump, SelectHotbar }
        public struct ActionPerformedArgs
        {
            public Actions Action;
        }

        PlayerCore player;
        public PlayerCore Player { get => player; }

        public event EventHandler<ActionPerformedArgs> OnActionPerform;

        public void OnSelectHotbar()
        {

        }

        void Awake()
        {
            player = GetComponent<PlayerCore>();
        }

        public void SelectHotbar(int index)
        {
            if (index < 0 || index > 9) return;

            OnActionPerform?.Invoke(this, new()
            {
                Action = Actions.SelectHotbar
            });

            ItemSlot slot = player.Inventory.Hotbar[index];
            Debug.Log($"Equipped hotbar slot {index}");
        }

        public void PerformPrimary()
        {
            // Attack
        }

        public void PerformSecondary()
        {
            
        }

        public void Jump(InputAction.CallbackContext context)
        {            
            //
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
            if (!player.CanPickUpItems) return;
            if (player.Inventory == null) return;
            
            if (player.Inventory.Bag.TryPutNearest(itemEntity.AsItem()))
            {
                Destroy(itemEntity.gameObject);
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
