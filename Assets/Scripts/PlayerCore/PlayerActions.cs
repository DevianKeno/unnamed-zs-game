using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UZSG.Systems;
using UZSG.Items;
using UZSG.Interactions;
using UZSG.Entities;
using UZSG.FPP;
using Cinemachine;
using UZSG.Inventory;

namespace UZSG.PlayerCore
{
    public enum PlayerStates {
        Idle, Run, Jump, Walk, Crouch, Equip, PerformPrimary, PerformSecondary, Hold
    }

    /// <summary>
    /// Handles the different actions the Player can do.
    /// </summary>
    public class PlayerActions : MonoBehaviour
    {
        [Header("Interact Size")]
        public float Radius;
        public float MaxInteractDistance;

        /// <summary>
        /// The interactable object the Player is currently looking at.
        /// </summary>
        IInteractable lookingAt;
        RaycastHit hit;
        Ray ray;
        
        [field: Header("Components")]
        Entities.Player player;
        PlayerInput input;
        InputAction primaryInput;
        InputAction secondaryInput;
        InputAction interactInput;
        InputAction inventoryInput;
        InputAction hotbarInput;

        internal void Init()
        {
        }

        void Awake()
        {
            player = GetComponent<Entities.Player>();
            input = GetComponent<PlayerInput>();
            
            primaryInput = input.actions.FindAction("Primary");
            secondaryInput = input.actions.FindAction("Secondary");
            interactInput = input.actions.FindAction("Interact");
            inventoryInput = input.actions.FindAction("Inventory");
            hotbarInput = input.actions.FindAction("Hotbar");
        }

        void Start()
        {
            Game.Tick.OnTick += Tick;
            
            primaryInput.Enable();
            secondaryInput.Enable();
            interactInput.Enable();
            inventoryInput.Enable();
            hotbarInput.Enable();

            /*  performed = Pressed or released
                started = Pressed
                canceled = Released
            */
            interactInput.performed += OnPerformInteract;         // F (default)
            inventoryInput.performed += OnPerformInventory;       // Tab/E (default)
            hotbarInput.performed += OnHotbarSelect;        // Tab/E (default)
            primaryInput.performed += OnPerformPrimary;           // LMB (default)
            secondaryInput.started += OnPerformSecondary;         // RMB (default)
            secondaryInput.canceled += OnPerformSecondary;         // RMB (default)

            // player.Inventory.Hotbar.OnChangeEquipped += HotbarChangeEquippedCallback;
        }

        void OnDisable()
        {
            Game.Tick.OnTick -= Tick;
            interactInput.Disable();
            inventoryInput.Disable();
        }

        #region Callbacks
        void OnHotbarSelect(InputAction.CallbackContext context)
        {
            if (!int.TryParse(context.control.displayName, out int index)) return;
            player.Inventory.SelectHotbarSlot(index);
        }

        void HotbarChangeEquippedCallback(object sender, Hotbar.ChangeEquippedArgs e)
        {
            player.FPP.Equip(e.Index);
        }
        
        void OnPerformInteract(InputAction.CallbackContext context)
        {
            if (lookingAt == null) return;            
            lookingAt.Interact(this, new InteractArgs());
        }

        /// <summary>
        /// I want the cam to lock and cursor to appear only when the key is released :P
        /// </summary>    
        void OnPerformInventory(InputAction.CallbackContext context)
        {
            Game.UI.InventoryUI.ToggleVisibility();
            // ToggleCameraControls(!Game.UI.InventoryUI.IsVisible);
        }

        void OnPerformPrimary(InputAction.CallbackContext context)
        {
            player.sm.ToState(player.sm.States[PlayerStates.PerformPrimary]);
        }

        void OnPerformSecondary(InputAction.CallbackContext context)
        {
            player.sm.ToState(player.sm.States[PlayerStates.PerformSecondary]);
        }
        #endregion

        void Tick(object sender, TickEventArgs e)
        {
            CheckLookingAt();
        }

        /// <summary>
        /// Maybe instead of firing every tick, this can just fire everytime the player's ray collides with an IInteractable object
        /// </summary>
        void CheckLookingAt()
        {
            // Cast a ray from the center of the screen
            ray = player.MainCamera.ScreenPointToRay(new Vector2(Screen.width / 2, Screen.height / 2));

            if (Physics.SphereCast(ray, Radius, out RaycastHit hit, MaxInteractDistance, LayerMask.GetMask("Interactable")))
            // if (Physics.Raycast(ray, out RaycastHit hit, MaxInteractDistance, LayerMask.GetMask("Interactable")))
            {
                lookingAt = hit.collider.gameObject.GetComponent<IInteractable>();

                Game.UI.InteractIndicator.Show(lookingAt);

            } else
            {
                lookingAt = null;
                Game.UI.InteractIndicator.Hide();
            }
        }        
        
        /// <summary>
        /// Visualizes the interaction size.
        /// </summary>
        void OnDrawGizmos()
        {
            Gizmos.DrawLine(ray.origin, ray.origin + ray.direction * (MaxInteractDistance));
            Gizmos.DrawWireSphere(ray.origin + ray.direction * (MaxInteractDistance + Radius), Radius);
        }

        /// <summary>
        /// Pick up item from ItemEntity and put in the inventory.
        /// There is currently no checking if inventory is full.
        /// </summary>
        public void PickUpItem(ItemEntity itemEntity)
        {
            if (!player.CanPickUpItems) return;
            if (player.Inventory == null) return;

            bool gotItem;
            Item item = itemEntity.AsItem();

            if (item.Type == ItemType.Weapon)
            {
                gotItem = player.Inventory.Hotbar.Mainhand.TryPutItem(item);

                if (WeaponData.TryGetWeaponData(item.Data, out WeaponData weaponData))
                {
                    player.FPP.Load(weaponData, 1);
                }

            } else if (item.Type == ItemType.Tool)
            {
                gotItem = player.Inventory.Hotbar.Offhand.TryPutItem(item);                

                // if (ToolData.TryGetToolData(item.Data, out ToolData toolData))
                // {
                //     _FPP.Load(toolData, 1);
                // }
                // // else try to put in other hotbar slots (3-0) and only if available
            } else
            {
                gotItem = player.Inventory.Bag.TryPutNearest(item);
            }

            if (gotItem) Destroy(itemEntity.gameObject);
        }
    }
}
