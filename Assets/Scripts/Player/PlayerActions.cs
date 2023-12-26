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

namespace UZSG.Player
{
    public enum PlayerStates { Idle, Run, Jump, Walk, Crouch, Equip, PerformPrimary, PerformSecondary, Hold }

    /// <summary>
    /// Represents the different actions the Player can do.
    /// </summary>
    [RequireComponent(typeof(PlayerCore))]
    public class PlayerActions : MonoBehaviour
    {
        public const float CamSensitivity = 0.32f;

        PlayerCore _player;
        public PlayerCore Player { get => _player; }
        [SerializeField] FPPHandler _FPP;
        [SerializeField] Camera cam;
        [SerializeField] bool _allowCamMovement = true;

        /// <summary>
        /// The interactable object the Player is currently looking at.
        /// </summary>
        IInteractable lookingAt;
        RaycastHit hit;
        Ray ray;
        public float sphereRadius = 0;
        public float interactMaxDistance = 1;
        
        PlayerInput _input;
        InputAction primaryInput;
        InputAction secondaryInput;
        InputAction interactInput;
        InputAction inventoryInput;
        InputAction hotbarInput;
        [SerializeField] CinemachineVirtualCamera vCam;
        CinemachinePOV _vCamPOV;

        void Awake()
        {
            _player = GetComponent<PlayerCore>();
            _input = GetComponent<PlayerInput>();
            _vCamPOV = vCam.GetCinemachineComponent<CinemachinePOV>();
            
            primaryInput = _input.actions.FindAction("Primary");
            secondaryInput = _input.actions.FindAction("Secondary");
            interactInput = _input.actions.FindAction("Interact");
            inventoryInput = _input.actions.FindAction("Inventory");
            hotbarInput = _input.actions.FindAction("Hotbar");
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
            interactInput.performed += OnInteractX;         // F (default)
            inventoryInput.performed += OnInventoryX;       // Tab/E (default)
            hotbarInput.performed += OnHotbarSelect;        // Tab/E (default)
            primaryInput.performed += OnPrimaryX;           // LMB (default)
            secondaryInput.started += OnSecondaryX;         // RMB (default)
            secondaryInput.canceled += OnSecondaryX;         // RMB (default)

            _player.Inventory.Hotbar.OnChangeEquipped += HotbarChangeEquippedCallback;
        }

        void OnDisable()
        {
            Game.Tick.OnTick -= Tick;
            interactInput.Disable();
            inventoryInput.Disable();
        }

        void OnHotbarSelect(InputAction.CallbackContext context)
        {
            if (!int.TryParse(context.control.displayName, out int index)) return;
            _player.Inventory.SelectHotbarSlot(index);
        }

        void HotbarChangeEquippedCallback(object sender, Hotbar.ChangeEquippedArgs e)
        {
            _FPP.Equip(e.Index);
        }
        
        void OnInteractX(InputAction.CallbackContext context)
        {
            if (lookingAt == null) return;            
            lookingAt.Interact(this, new InteractArgs());
        }

        /// <summary>
        /// I want the cam to lock and cursor to appear only when the key is released :P
        /// </summary>    
        void OnInventoryX(InputAction.CallbackContext context)
        {
            Game.UI.InventoryUI.ToggleVisibility();
            AllowCameraMovement(!Game.UI.InventoryUI.IsVisible);
        }

        void OnPrimaryX(InputAction.CallbackContext context)
        {
            _player.sm.ToState(_player.sm.States[PlayerStates.PerformPrimary]);
        }

        void OnSecondaryX(InputAction.CallbackContext context)
        {
            _player.sm.ToState(_player.sm.States[PlayerStates.PerformSecondary]);
        }

        void Tick(object sender, TickEventArgs e)
        {
            CheckLookingAt();
        }

        void CheckLookingAt()
        {
            // Cast a ray from the center of the screen
            ray = cam.ScreenPointToRay(new Vector2(Screen.width / 2, Screen.height / 2));

            if (Physics.SphereCast(ray, sphereRadius, out RaycastHit hit, interactMaxDistance, LayerMask.GetMask("Interactable")))
            {
                lookingAt = hit.collider.gameObject.GetComponent<IInteractable>();

                if (hit.collider.CompareTag("Item"))
                {
                    Game.UI.InteractIndicator.Show(lookingAt);
                }
            } else
            {
                lookingAt = null;
                Game.UI.InteractIndicator.Hide();
            }
        }        
        
        // void OnDrawGizmos()
        // {
        //     Gizmos.DrawLine(ray.origin, ray.origin + ray.direction * (interactMaxDistance + sphereRadius));
        //     Gizmos.DrawWireSphere(ray.origin + ray.direction * (interactMaxDistance + sphereRadius), sphereRadius);
        //     Gizmos.DrawWireSphere(ray.origin + ray.direction * (interactMaxDistance + sphereRadius), sphereRadius);
        // }

        /// <summary>
        /// Pick up item from ItemEntity and put in the inventory.
        /// There is currently no checking if inventory is full.
        /// </summary>
        public void PickUpItem(ItemEntity itemEntity)
        {
            if (!_player.CanPickUpItems) return;
            if (_player.Inventory == null) return;

            bool gotItem;
            Item item = itemEntity.AsItem();

            if (item.Type == ItemType.Weapon)
            {
                gotItem = _player.Inventory.Hotbar.Mainhand.TryPutItem(item);

                if (WeaponData.TryGetWeaponData(item.Data, out WeaponData weaponData))
                {
                    _FPP.Load(weaponData, 1);
                }

            } else if (item.Type == ItemType.Tool)
            {
                gotItem = _player.Inventory.Hotbar.Offhand.TryPutItem(item);                

                // if (ToolData.TryGetToolData(item.Data, out ToolData toolData))
                // {
                //     _FPP.Load(toolData, 1);
                // }
                // // else try to put in other hotbar slots (3-0) and only if available
            } else
            {
                gotItem = _player.Inventory.Bag.TryPutNearest(item);
            }

            if (gotItem) Destroy(itemEntity.gameObject);
        }
        
        public void AllowCameraMovement(bool value)
        {
            if (value)
            {
                _allowCamMovement = true;
                _vCamPOV.m_VerticalAxis.m_MaxSpeed = CamSensitivity;
                _vCamPOV.m_HorizontalAxis.m_MaxSpeed = CamSensitivity;
            } else
            {
                _allowCamMovement = false;
                _vCamPOV.m_VerticalAxis.m_MaxSpeed = 0f;
                _vCamPOV.m_HorizontalAxis.m_MaxSpeed = 0f;
            }
        }
    }
}
