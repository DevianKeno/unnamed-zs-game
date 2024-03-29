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
    /// <summary>
    /// Handles the different actions the Player can do.
    /// </summary>
    public class PlayerActions : MonoBehaviour
    {
        [Header("Interact Size")]
        public float Radius;
        public float MaxInteractDistance;
        public float HoldThresholdMs = 200f;
        float _holdTimer;
        bool leftClicked;
        bool rightClicked;
        bool heldClick;
        
        /// <summary>
        /// The interactable object the Player is currently looking at.
        /// </summary>
        IInteractable lookingAt;
        RaycastHit hit;
        Ray ray;
        
        [field: Header("Components")]
        Player player;
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
            player = GetComponent<Player>();
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
            interactInput.performed += OnPerformInteract;        // F (default)
            inventoryInput.performed += OnPerformInventory;      // Tab/E (default)
            hotbarInput.performed += OnHotbarSelect;             // Tab/E (default)

            primaryInput.started += OnStartPrimary;          // LMB (default)
            primaryInput.canceled += OnCancelPrimary;          // LMB (default)

            secondaryInput.started += OnStartSecondary;        // RMB (default)
            secondaryInput.canceled += OnCancelSecondary;       // RMB (default)

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
  
        void Update()
        {
            if (leftClicked)
            {
                _holdTimer += Time.deltaTime;

                if (_holdTimer > HoldThresholdMs / 1000f)
                {
                    leftClicked = false;
                    player.smAction.ToState(ActionStates.PrimaryHold);
                }

            } else if (rightClicked)
            {
                _holdTimer += Time.deltaTime;

                if (_holdTimer > HoldThresholdMs / 1000f)
                {
                    rightClicked = false;
                    player.smAction.ToState(ActionStates.SecondaryHold);
                }
            }
        }

        void OnStartPrimary(InputAction.CallbackContext c)
        {
            leftClicked = true;
            _holdTimer = 0f;
        }

        void OnCancelPrimary(InputAction.CallbackContext c)
        {
            if (_holdTimer < HoldThresholdMs / 1000f)
            {
                leftClicked = false;
                player.smAction.ToState(ActionStates.Primary);
            }
        }

        void OnStartSecondary(InputAction.CallbackContext c)
        {
            rightClicked = true;
            _holdTimer = 0f;
        }

        void OnCancelSecondary(InputAction.CallbackContext c)
        {
            if (_holdTimer < HoldThresholdMs / 1000f)
            {
                rightClicked = false;
                player.smAction.ToState(ActionStates.Secondary);
            }
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
                gotItem = player.Inventory.Mainhand.TryPutItem(item);

                if (gotItem)
                {
                    if (WeaponData.TryGetWeaponData(item.Data, out WeaponData weaponData))
                    {
                        player.FPP.LoadModel(weaponData, 1);
                        player.FPP.Equip(1);
                    }
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
