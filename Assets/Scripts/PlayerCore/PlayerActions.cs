using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.InputSystem;
using MEC;

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
        
        bool _hadLeftClicked;
        bool _hadRightClicked;
        bool _isHoldingLeftClick;
        bool _isHoldingRightClick;
        bool _isBusy;
        bool _allowInteractions = true;
        float _holdTimer;
        /// <summary>
        /// The interactable object the Player is currently looking at.
        /// </summary>
        IInteractable lastLookedAt;
        IInteractable lookingAt;
        Ray ray;
        InteractionIndicator interactionIndicator;

        public event Action<Item> OnPickupItem;
        public event Action<ILookable> OnLookAtSomething;
        
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
            
            backInput = Game.Main.GetInputAction("Back", "Global");
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

        void Tick(TickInfo e)
        {
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
                    if (interactable != null && interactable.AllowInteractions)
                    {
                        lookingAt?.OnLookExit();
                        lookingAt = interactable;
                        lookingAt.OnLookEnter();
                        interactionIndicator.Indicate(lookingAt);
                        // OnLookAtSomething?.Invoke(interactable);
                        return;
                    }
                }

                var lookable = hit.collider.GetComponentInParent<ILookable>();
                if (lookable != null && lookable.AllowInteractions)
                {
                    OnLookAtSomething?.Invoke(lookable);
                    return;
                }
            }

            interactionIndicator.Hide();
            lookingAt?.OnLookExit();
            lookingAt = null;
            OnLookAtSomething?.Invoke(null);
        }


        #region Pickup action
        
        public enum PickupStatus {
            Started, Finished, Canceled 
        }
        RadialProgressUI pickupRingUI;
        float _pickupDeltaTime;
        MEC.CoroutineHandle pickupTimerHandle;
        /// <summary>
        /// The position of the object the Player is currently interacting with.
        /// To compare distance and cancel when too far.
        /// </summary>
        Vector3 _actionPosition;
        float _actionMaxDistance;
        
        InputAction backInput;

        public void StartPickupRoutine(Objects.ResourcePickup resource, Action<PickupStatus> onTimerNotify = null)
        {
            if (_isBusy) return;

            var pickupTime = resource.ResourceData.PickupDuration;
            if (pickupTime > 0)
            {
                _isBusy = true;
                DisableControlsOnPickupResource();
                pickupRingUI = Game.UI.Create<RadialProgressUI>("Pickup Ring UI");
                pickupRingUI.TotalTime = pickupTime;
                pickupRingUI.Progress = 0f;
                _pickupDeltaTime = 0f;
                _actionPosition = resource.Position;
                _actionMaxDistance = resource.ResourceData.MaxInteractDistance;

                #region TODO: cancel action on global back Input [ESC]
                // backInput.performed += CancelCurrentAction;
                #endregion

                Timing.KillCoroutines(pickupTimerHandle);
                pickupTimerHandle = Timing.RunCoroutine(
                    UpdatePickupRoutine(pickupTime, onTimerNotify));
            }
            else /// instant
            {
                FinishPickupRoutine(onTimerNotify);
            }
        }

        IEnumerator<float> UpdatePickupRoutine(float duration, Action<PickupStatus> onTimerNotify)
        {
            while (_pickupDeltaTime < duration)
            {
                _pickupDeltaTime += Time.deltaTime;
                pickupRingUI.Progress = _pickupDeltaTime / duration;
                
                if (Vector3.Distance(_actionPosition, Player.Position) > _actionMaxDistance)
                {
                    /// player is now too far away from the object
                    CancelPickupResource(onTimerNotify);
                    yield break;
                }
                if (_pickupDeltaTime >= duration)
                {
                    FinishPickupRoutine(onTimerNotify);
                    yield break;
                }
                yield return Timing.WaitForOneFrame;
            }
        }

        void FinishPickupRoutine(Action<PickupStatus> onTimerNotify)
        {
            _isBusy = false;
            pickupRingUI.Destroy();
            pickupRingUI = null;
            EnableControlsOnPickupResource();
            onTimerNotify?.Invoke(PickupStatus.Finished);
            onTimerNotify = null;
        }
        
        void CancelPickupResource(Action<PickupStatus> onTimerNotify)
        {
            Timing.KillCoroutines(pickupTimerHandle);
            if (pickupRingUI != null)
            {
                pickupRingUI.Destroy();
            }
            
            _isBusy = false;
            pickupRingUI.Destroy();
            pickupRingUI = null;
            EnableControlsOnPickupResource();
            onTimerNotify?.Invoke(PickupStatus.Canceled);
            onTimerNotify = null;
        }

        void EnableControlsOnPickupResource()
        {
            Enable();
        }

        void DisableControlsOnPickupResource()
        {
            Disable();
        }

        #endregion


        #region Input callbacks

        void OnNumberSlotSelect(InputAction.CallbackContext context)
        {
            if (!int.TryParse(context.control.displayName, out int index)) return;

            var slot = Player.Inventory.GetEquipmentOrHotbarSlot(index);
            if (slot == null)
            {
                Game.Console.LogAndUnityLog($"Tried to access Hotbar Slot {index}, but it's not available yet (wear a toolbelt or smth.)");
                return;
            }
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


        #region Public methods

        /// <summary>
        /// Pick up item from ItemEntity and put in the inventory.
        /// </summary>
        public void PickUpItem(ItemEntity itemEntity)
        {
            if (!Player.CanPickUpItems) return;
            if (Player.FPP.IsPerforming) return;

            bool gotItem; /// whether if the player had successfully picked up the item
            Item item = itemEntity.Item;
        
            if (item.Data.Type == ItemType.Weapon) /// put in equipment (if possible)
            {
                if (Player.Inventory.Equipment.TryEquipWeapon(item, out EquipmentIndex index))
                {
                    Player.FPP.HoldItem(item.Data);
                    OnPickupItem?.Invoke(item);
                    DestroyPickupedItem(itemEntity);
                    return;
                }
            }
            else if (item.Data.Type == ItemType.Tool) /// put in hotbar (if possible)
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

        /// <summary>
        /// Enable Player actions. This include:
        /// - Picking up items
        /// - Interacting with pickups/objects
        /// - Equipping items via hotbar
        /// - etc. idk
        /// </summary>
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

        #endregion


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
