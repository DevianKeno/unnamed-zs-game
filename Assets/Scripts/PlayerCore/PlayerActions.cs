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
using System.Collections;

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
        
        bool _inhibitActions;
        float _holdTimer;
        bool _hadLeftClicked;
        bool _hadRightClicked;
        bool _isHoldingLeftClick;
        bool _isHoldingRightClick;
        
        [SerializeField] PlayerLookRaycaster lookRaycaster;
        /// <summary>
        /// The interactable object the Player is currently looking at.
        /// </summary>
        IInteractable lastLookedAt;
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
            interactionIndicator = Game.UI.Create<InteractionIndicator>("Interaction Indicator", show: false);
            lookRaycaster.OnLookEnter += HandleLookEnter;
            lookRaycaster.OnLookExit += HandleLookExit;

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
        
        void HandleLookEnter(Collider collider)
        {
            if (collider.CompareTag("Interactable"))
            {
                lookingAt?.OnLookExit();
                lookingAt = collider.GetComponentInParent<IInteractable>(); /// look at new
                if (lookingAt == null) return; /// what are you doing setting an object's layer in interactable but not add a IInteractable component???
                lookingAt.OnLookEnter();
                interactionIndicator.Indicate(lookingAt);
                return;
            }
        }

        void HandleLookExit(Collider collider)
        {
            interactionIndicator.Hide();
            lookingAt?.OnLookExit();
            lookingAt = null;
        }


        #region Input callbacks

        void OnHotbarSelect(InputAction.CallbackContext context)
        {
            if (!int.TryParse(context.control.displayName, out int index)) return;

            var hotbarIndex = (HotbarIndex) index;

            // Player.Inventory.SelectHotbarSlot(index);
            Player.FPP.EquipHotbarIndex(hotbarIndex);
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
            if (_inhibitActions) return;
            if (lookingAt == null) return;

            StartCoroutine(InteractCoroutine());
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
        
        IEnumerator InteractCoroutine()
        {
            _inhibitActions = true;
            interactionIndicator.Hide();
            lookingAt.Interact(this, new());
            yield return new WaitForSeconds(0.1f); /// interact cooldown
            _inhibitActions = false;
        }

        /// <summary>
        /// Pick up item from ItemEntity and put in the inventory.
        /// </summary>
        public void PickUpItem(ItemEntity itemEntity)
        {
            if (!Player.CanPickUpItems) return;
            if (Player.Inventory == null) return;

            bool gotItem; /// whether if the player had successfully picked up the item
            Item item = itemEntity.Item;

            if (item.Data.Type == ItemType.Weapon)
            {
                gotItem = Player.Inventory.TryEquipWeapon(item, out HotbarIndex index);

                if (gotItem)
                {
                    Player.FPP.LoadFPPItem(item.Data, index, equip: true);
                    Player.FPP.InitializeHeldItem(item, index, () =>
                    {
                        Player.FPP.EquipHotbarIndex(index);
                    });
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
