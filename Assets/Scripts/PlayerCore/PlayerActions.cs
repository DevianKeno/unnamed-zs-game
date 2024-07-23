using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.InputSystem;

using UZSG.Systems;
using UZSG.Items;
using UZSG.Items.Weapons;
using UZSG.Interactions;
using UZSG.Entities;
using UZSG.Inventory;
using UZSG.UI;

namespace UZSG.Players
{
    /// <summary>
    /// Handles the different actions the Player can do.
    /// </summary>
    public class PlayerActions : MonoBehaviour
    {
        public Player Player;

        [Header("Interaction Size")]
        public float Radius;
        public float MaxInteractDistance;
        public float HoldThresholdMilliseconds = 200f;
        
        float _holdTimer;
        bool _hadLeftClicked;
        bool _hadRightClicked;
        bool _isHoldingLeftClick;
        bool _isHoldingRightClick;
        
        /// <summary>
        /// The interactable object the Player is currently looking at.
        /// </summary>
        IInteractable lookingAt;
        Ray ray;
        InteractionIndicator interactionIndicator;

        public event Action<Item> OnPickupItem;
        
        InputActionMap actionMap;
        Dictionary<string, InputAction> inputs = new();
        
        void Awake()
        {
            Player = GetComponent<Player>();
        }

        internal void Initialize()
        {
            InitializeInputs();
            interactionIndicator = Game.UI.Create<InteractionIndicator>("interaction_indicator");

            Game.Tick.OnTick += Tick;
        }

        void InitializeInputs()
        {
            actionMap = Game.Main.GetActionMap("Player");
            inputs = Game.Main.GetActionsFromMap(actionMap);
        
            inputs["Primary Action"].started += OnStartPrimaryAction;           // LMB (default)
            inputs["Primary Action"].canceled += OnCancelPrimaryAction;         // LMB (default)

            inputs["Secondary Action"].started += OnStartSecondaryAction;       // RMB (default)
            inputs["Secondary Action"].canceled += OnCancelSecondaryAction;     // RMB (default)
            
            inputs["Reload"].performed += OnPerformReload;       // R (default)

            inputs["Interact"].performed += OnPerformInteract;                  // F (default)
            inputs["Interact"].Enable();
            inputs["Hotbar"].performed += OnHotbarSelect;               // Tab/E (default)

            inputs["Unholster"].performed += OnUnholster;               // X (default)
        }

        void Update()
        {
            if (_hadLeftClicked)
            {
                _holdTimer += Time.deltaTime;

                if (_holdTimer > HoldThresholdMilliseconds / 1000f)
                {
                    _isHoldingLeftClick = true;
                    Player.ActionStateMachine.ToState(ActionStates.PrimaryHold);
                }
            }
            else if (_hadRightClicked)
            {
                _holdTimer += Time.deltaTime;

                if (_holdTimer > HoldThresholdMilliseconds / 1000f)
                {
                    _isHoldingRightClick = true;
                    Player.ActionStateMachine.ToState(ActionStates.SecondaryHold);
                }
            }
        }

        #region Callbacks

        void OnHotbarSelect(InputAction.CallbackContext context)
        {
            if (!int.TryParse(context.control.displayName, out int index)) return;

            if (Player.FPP.CurrentlyEquippedIndex == (HotbarIndex) index)
            {
                Player.Inventory.SelectHotbarSlot(0);
                Player.FPP.Unholster();
            }
            else
            {
                Player.Inventory.SelectHotbarSlot(index);
                Player.FPP.EquipIndex((HotbarIndex) index);
            }
        }

        void OnPerformReload(InputAction.CallbackContext context)
        {
            Player.FPP.PerformReload();
        }

        void OnUnholster(InputAction.CallbackContext context)
        {
            Player.FPP.Unholster();
        }
        
        void OnPerformInteract(InputAction.CallbackContext context)
        {
            if (lookingAt == null) return;

            lookingAt.Interact(this, new InteractArgs());
        }

        void OnStartPrimaryAction(InputAction.CallbackContext c)
        {
            _holdTimer = 0f;
            _hadLeftClicked = true;
        }

        void OnCancelPrimaryAction(InputAction.CallbackContext c)
        {
            _hadLeftClicked = false;
            if (_isHoldingLeftClick)
            {
                _isHoldingLeftClick = false;
                Player.ActionStateMachine.ToState(ActionStates.PrimaryRelease);
            }
            else
            {
                Player.ActionStateMachine.ToState(ActionStates.Primary);
            }
        }

        void OnStartSecondaryAction(InputAction.CallbackContext c)
        {
            _holdTimer = 0f;
            _hadRightClicked = true;
        }

        void OnCancelSecondaryAction(InputAction.CallbackContext c)
        {
            _hadRightClicked = false;
            if (_isHoldingRightClick)
            {
                _isHoldingRightClick = false;
                Player.ActionStateMachine.ToState(ActionStates.SecondaryRelease);
            }
            else
            {
                Player.ActionStateMachine.ToState(ActionStates.Secondary);
            }
        }

        #endregion

        void Tick(TickInfo e)
        {
            CheckLookingAt();
        }

        /// <summary>
        /// Maybe instead of firing every tick, this can just fire everytime the player's ray collides with an IInteractable object
        /// </summary>
        void CheckLookingAt()
        {
            /// Cast a ray from the center of the screen
            var viewportRect = new Vector2(Screen.width / 2, Screen.height / 2);
            ray = Player.MainCamera.ScreenPointToRay(viewportRect);

            if (Physics.SphereCast(ray, Radius, out RaycastHit hit, MaxInteractDistance, LayerMask.GetMask("Interactable")))
            {
                if (hit.collider.gameObject.TryGetComponent(out lookingAt))
                {
                    interactionIndicator.Indicate(lookingAt);
                }
            }
            else
            {
                lookingAt = null;
                interactionIndicator.Hide();
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
        /// TODO: There is currently no checking if inventory is full.
        /// </summary>
        public void PickUpItem(ItemEntity itemEntity)
        {
            if (!Player.CanPickUpItems) return;
            if (Player.Inventory == null) return;

            bool gotItem; /// if the player had picked up the item
            Item item = itemEntity.AsItem();

            if (item.Type == ItemType.Weapon)
            {
                gotItem = Player.Inventory.TryEquipWeapon(item, out HotbarIndex index);

                if (gotItem)
                {
                    if (WeaponData.TryGetWeaponData(item.Data, out WeaponData weaponData))
                    {
                        Player.FPP.LoadAndEquip(weaponData, index);
                    }
                }
            }
            else /// generic item
            {
                if (Player.Inventory.IsFull)
                {
                    /// Prompt inventory full
                    var msg = $"Can't pick up item. Inventory full";
                    Game.Console.Log(msg);
                    Debug.Log(msg);
                    return;
                }
                gotItem = Player.Inventory.Bag.TryPutNearest(item);
            }

            if (gotItem)
            {
                OnPickupItem?.Invoke(item);
                Game.Entity.Kill(itemEntity);
            }
        }
    }
}
