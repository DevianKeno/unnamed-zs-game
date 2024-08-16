using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.InputSystem;

using UZSG.Systems;
using UZSG.Data;
using UZSG.UI;
using UZSG.Interactions;
using UZSG.Entities;
using UZSG.Inventory;
using UZSG.Items;

namespace UZSG.Players
{
    /// <summary>
    /// Handles the different actions the Player can do.
    /// </summary>
    public class PlayerActions : MonoBehaviour
    {
        public Player Player;
        [Space]

        [Header("Interaction Size")]
        public float Radius;
        [Range(0.1f, 5f)]
        public float MaxInteractDistance;
        public float HoldThresholdMilliseconds = 200f;
        
        float _holdTimer;
        bool _hadLeftClicked;
        bool _hadRightClicked;
        bool _isHoldingLeftClick;
        bool _isHoldingRightClick;
        bool _allowInteractions = true;
        /// <summary>
        /// The interactable object the Player is currently looking at.
        /// </summary>
        IInteractable lastLookedAt;
        IInteractable lookingAt;
        Ray ray;
        InteractionIndicator interactionIndicator;

        public event Action<Item> OnPickupItem;
        
        InputActionMap actionMap;
        public InputActionMap ActionMap => actionMap;
        Dictionary<string, InputAction> inputs = new();
        public Dictionary<string, InputAction> Inputs => inputs;
        
        void Awake()
        {
            Player = GetComponent<Player>();
        }

        internal void Initialize()
        {
            InitializeInputs();
            interactionIndicator = Game.UI.Create<InteractionIndicator>("Interaction Indicator", show: false);

            Game.Tick.OnTick += Tick;
        }

        void InitializeInputs()
        {
            actionMap = Game.Main.GetActionMap("Player Actions");
            inputs = Game.Main.GetActionsFromMap(actionMap);
        
            inputs["Primary Action"].started += OnStartPrimaryAction;           // LMB (default)
            inputs["Primary Action"].canceled += OnCancelPrimaryAction;         // LMB (default)

            inputs["Secondary Action"].started += OnStartSecondaryAction;       // RMB (default)
            inputs["Secondary Action"].canceled += OnCancelSecondaryAction;     // RMB (default)
            
            inputs["Reload"].performed += OnPerformReload;       // R (default)

            inputs["Interact"].performed += OnPerformInteract;                  // F (default)
            inputs["Interact"].Enable();
            inputs["Hotbar"].performed += OnNumberSlotSelect;               // Tab/E (default)

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

        void FixedUpdate()
        {
            InteractionSphereCast();
        }

        public void Enable()
        {
            actionMap.Enable();
            _allowInteractions = true;
        }

        public void Disable()
        {
            actionMap.Disable();
            _allowInteractions = false;
        }

        public void SetControl(string name, bool enabled)
        {
            if (inputs.ContainsKey(name))
            {
                if (enabled)
                {
                    inputs[name].Enable();
                }
                else
                {
                    inputs[name].Disable();
                }
            }
        }

        void InteractionSphereCast()
        {
            if (!_allowInteractions) return;

            var viewportCenter = new Vector2(Screen.width / 2, Screen.height / 2);
            ray = Player.MainCamera.ScreenPointToRay(viewportCenter);
            
            foreach (var hit in Physics.SphereCastAll(ray, Radius, MaxInteractDistance))
            {
                if (hit.collider != null && hit.collider.CompareTag("Interactable"))
                {
                    var interactable = hit.collider.GetComponentInParent<IInteractable>();
                    if (interactable != null)
                    {
                        lookingAt?.OnLookExit();
                        lookingAt = interactable;
                        lookingAt.OnLookEnter();
                        interactionIndicator.Indicate(lookingAt);
                        return;
                    }
                }
            }

            interactionIndicator.Hide();
            lookingAt?.OnLookExit();
            lookingAt = null;
        }


        #region Input callbacks

        void OnNumberSlotSelect(InputAction.CallbackContext context)
        {
            if (!int.TryParse(context.control.displayName, out int index)) return;

            var slot = Player.Inventory.GetEquipmentOrHotbarSlot(index);
            if (slot.HasItem)
            {
                Player.FPP.EquipHeldItem(slot.Item.Data.Id);
                // Player.Inventory.SelectHotbarSlot(index);
            }
        }

        void OnUnholster(InputAction.CallbackContext context)
        {
            // Player.Inventory.SelectHotbarSlot(0);
            Player.FPP.Unholster();
        }

        void OnPerformReload(InputAction.CallbackContext context)
        {
            Player.FPP.PerformReload();
        }
        
        void OnPerformInteract(InputAction.CallbackContext context)
        {
            if (lookingAt == null) return;

            interactionIndicator.Hide();
            lookingAt.Interact(Player, new());
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
        }

        /// <summary>
        /// Pick up item from ItemEntity and put in the inventory.
        /// </summary>
        public void PickUpItem(ItemEntity itemEntity)
        {
            if (!Player.CanPickUpItems) return;
            if (Player.FPP.IsPerforming) return;

            bool gotItem; /// whether if the player had successfully picked up the item
            Item item = itemEntity.Item;
        
            if (item.Data.Type == ItemType.Weapon)
            {
                if (Player.Inventory.Equipment.TryEquipWeapon(item, out EquipmentIndex index))
                {
                    Player.FPP.HoldItem(item.Data);
                    OnPickupItem?.Invoke(item);
                    DestroyPickupedItem(itemEntity);
                    return;
                }
            }
            else if (item.Data.Type == ItemType.Tool)
            {
                if (Player.Inventory.Hotbar.TryPutNearest(item))
                {
                    Player.FPP.HoldItem(item.Data);
                    OnPickupItem?.Invoke(item);
                    DestroyPickupedItem(itemEntity);
                    return;
                }
            }
            
            /// Store generic items or items can't hold
            if (Player.Inventory.IsFull)
            {
                /// Prompt inventory full SUBJECT TO CHAZNGE
                var msg = $"Can't pick up item. Inventory full";
                Game.Console.LogAndUnityLog(msg);
                return;
            }

            gotItem = Player.Inventory.Bag.TryPutNearest(item);
            if (gotItem)
            {
                    lookingAt = null;
                OnPickupItem?.Invoke(item);
                DestroyPickupedItem(itemEntity);
            }
        }

        void DestroyPickupedItem(Entity item)
        {
            Game.Entity.Kill(item);
        }
        
        /// <summary>
        /// Visualizes the interaction size.
        /// </summary>
        void OnDrawGizmos()
        {
            Gizmos.DrawLine(ray.origin, ray.origin + ray.direction * (MaxInteractDistance));
            Gizmos.DrawWireSphere(ray.origin + ray.direction * (MaxInteractDistance + Radius), Radius);
        }
    }
}
