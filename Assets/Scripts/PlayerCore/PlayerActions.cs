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
        public Player Player { get; private set; }
        [Space]

        [Header("Interaction Size")]
        public float Radius;
        [Range(0.1f, 5f)]
        public float MaxInteractDistance;
        public float HoldThresholdMilliseconds = 200f;
        
        public bool IsBusy { get; protected set; }
        bool _hadLeftClicked;
        bool _hadRightClicked;
        bool _isHoldingLeftClick;
        bool _isHoldingRightClick;
        bool _allowInteractions = true;
        float _holdTimer;
        /// <summary>
        /// The interactable object the Player is currently looking at.
        /// </summary>
        IInteractable _lastLookedAt;
        IInteractable _lookingAt;
        Ray _ray;
        InteractionIndicator interactionIndicator;
        RigidbodyConstraints _savedRigidbodyConstraints;
        PauseMenuWindow pauseMenu;


        #region Actions events

        public event Action<Item> OnPickupItem;
        public event Action<ILookable> OnLookAtSomething;
        /// <summary>
        /// This should be passed with context.
        /// </summary>
        public event Action<InteractContext> OnInteract;
        public event Action<VehicleInteractContext> OnInteractVehicle;

        #endregion
        

        InputActionMap actionMap;
        public InputActionMap ActionMap => actionMap;
        Dictionary<string, InputAction> inputs = new();
        public Dictionary<string, InputAction> Inputs => inputs;


        #region Initializing methods

        void Awake()
        {
            Player = GetComponent<Player>();
        }

        internal void Initialize()
        {
            _savedRigidbodyConstraints = Player.Rigidbody.constraints;
            InitializeEvents();
            InitializeUI();
            InitializeInputs();

            Game.Tick.OnTick += Tick;
        }

        void InitializeEvents()
        {
            Game.UI.OnWindowOpened += (window) =>
            {
                Disable();
            };
            Game.UI.OnWindowClosed += (window) =>
            {
                Enable();
            };
        }

        void InitializeUI()
        {
            interactionIndicator = Game.UI.Create<InteractionIndicator>("Interaction Indicator", show: false);
        }

        void InitializeInputs()
        {
            actionMap = Game.Main.GetActionMap("Player Actions");
            inputs = Game.Main.GetActionsFromMap(actionMap);
        
            inputs["Primary Action"].started += OnStartPrimaryAction;           // LMB (default)
            inputs["Primary Action"].canceled += OnCancelPrimaryAction;         // LMB (default)

            inputs["Secondary Action"].started += OnStartSecondaryAction;       // RMB (default)
            inputs["Secondary Action"].canceled += OnCancelSecondaryAction;     // RMB (default)
            
            inputs["Interact"].performed += OnPerformInteract;                  // F (default)
            inputs["Interact"].Enable();

            inputs["Pause"] = Game.Main.GetInputAction("Pause", "World");
            inputs["Pause"].performed += OnInputPause;
            inputs["Pause"].Enable();
            
            backInput = Game.Main.GetInputAction("Back", "Global");
        }

        #endregion


        void OnDestroy()
        {
            Game.Tick.OnTick -= Tick;
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
            DetectOutOfBounds();
        }

        void Tick(TickInfo e)
        {
        }

        void InteractionSphereCast()
        {
            if (!_allowInteractions) return;
            if (IsBusy) return;

            var viewportCenter = new Vector2(Screen.width / 2, Screen.height / 2);
            _ray = Player.MainCamera.ScreenPointToRay(viewportCenter);
            
            foreach (var hit in Physics.SphereCastAll(_ray, Radius, MaxInteractDistance))
            {
                // if (hit.collider != null && hit.collider.CompareTag("Interactable"))
                if (hit.collider.CompareTag("Interactable"))
                {
                    var interactable = hit.collider.GetComponentInParent<IInteractable>();
                    if (interactable != null && interactable.AllowInteractions)
                    {
                        _lookingAt?.OnLookExit();
                        _lookingAt = interactable;
                        _lookingAt.OnLookEnter();
                        interactionIndicator.Indicate(_lookingAt);
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
            _lookingAt?.OnLookExit();
            _lookingAt = null;
            OnLookAtSomething?.Invoke(null);
        }

        void DetectOutOfBounds()
        {
            if (!Player.Controls.IsFalling) return;
            
            if (Player.Position.y < -32f)
            if (Physics.Raycast(new Vector3(Player.Position.x, 300f, Player.Position.z), -Vector3.up, out var hit, 999f))
            if (hit.collider.TryGetComponent<Terrain>(out var terrain))
            {
                Player.Position = new(Player.Position.x, hit.point.y + 1f, Player.Position.z);
            }
        }


        #region Pickup action
        
        public enum PickupEventStatus {
            Started, Performed, Canceled 
        }

        RadialProgressUI pickupRingUI;
        float _pickupTimer;
        MEC.CoroutineHandle pickupTimerHandle;
        /// <summary>
        /// The position of the object the Player is currently interacting with.
        /// To compare distance and cancel when too far.
        /// </summary>
        Vector3 _actionPosition;
        
        InputAction backInput;

        /// <summary>
        /// Routine for picking up 'Resource Pickups'.
        /// </summary>
        public void StartPickupRoutine(Objects.ResourcePickup resource)
        {
            if (resource == null) return;

            IsBusy = true;
            OnInteract?.Invoke(new InteractContext()
            {
                Actor = Player,
                Interactable = resource,
                Phase = InteractPhase.Started,
            });

            var pickupTime = resource.ResourceData.PickupDuration;
            if (pickupTime > 0)
            {
                DisableControlsOnPickupResource();
                pickupRingUI = Game.UI.Create<RadialProgressUI>("Pickup Ring UI");
                pickupRingUI.TotalTime = pickupTime;
                pickupRingUI.Progress = 0f;
                _pickupTimer = 0f;
                _actionPosition = resource.Position;

                #region TODO: cancel action on global back Input [ESC]
                // backInput.performed += CancelCurrentAction;
                #endregion

                Timing.KillCoroutines(pickupTimerHandle);
                pickupTimerHandle = Timing.RunCoroutine(UpdatePickupRoutine(resource, pickupTime));
            }
            else /// instant
            {
                FinishPickupRoutine(resource);
            }
        }

        IEnumerator<float> UpdatePickupRoutine(Objects.ResourcePickup resource, float duration)
        {
            while (_pickupTimer < duration)
            {
                _pickupTimer += Time.deltaTime;
                pickupRingUI.Progress = _pickupTimer / duration;
                
                if (Vector3.Distance(_actionPosition, Player.Position) > MaxInteractDistance)
                {
                    /// player is now too far away from the object
                    CancelPickupResource(resource);
                    yield break;
                }
                if (_pickupTimer >= duration)
                {
                    FinishPickupRoutine(resource);
                    yield break;
                }

                yield return Timing.WaitForOneFrame;
            }
        }

        void FinishPickupRoutine(Objects.ResourcePickup resource)
        {
            pickupRingUI?.Destroy();
            pickupRingUI = null;
            EnableControlsOnPickupResource();
            OnInteract?.Invoke(new InteractContext()
            {
                Actor = Player,
                Interactable = resource,
                Phase = InteractPhase.Finished,
            });
            IsBusy = false;
        }
        
        void CancelPickupResource(Objects.ResourcePickup resource)
        {
            Timing.KillCoroutines(pickupTimerHandle);
            pickupRingUI?.Destroy();
            pickupRingUI = null;
            EnableControlsOnPickupResource();
            OnInteract?.Invoke(new InteractContext()
            {
                Actor = Player,
                Interactable = resource,
                Phase = InteractPhase.Canceled,
            });
            IsBusy = false;
        }

        void EnableControlsOnPickupResource()
        {
            Enable();
        }

        void DisableControlsOnPickupResource()
        {
            Disable();
        }

        void SetPlayerPaused(bool pause)
        {
            if (pause) /// pause player
            {
                Game.UI.SetCursorVisible(true);
                Player.Controls.Disable();
                Player.FPP.Camera.ToggleControls(false);
                this.Disable();
                
                pauseMenu?.Destroy(invokeOnHideEvent: false);
                pauseMenu = Game.UI.Create<PauseMenuWindow>("Pause Menu UI");
                pauseMenu.OnClosed += UnpauseWorld;
            }
            else /// unpause player
            {
                Game.UI.SetCursorVisible(false);
                Player.Controls.Enable();
                Player.FPP.Camera.ToggleControls(true);
                this.Enable();
                
                pauseMenu?.Destroy(invokeOnHideEvent: false);
                pauseMenu = null;
            }
        }

        #endregion


        #region Input callbacks
        
        void OnPerformInteract(InputAction.CallbackContext context)
        {
            if (IsBusy) return;
            if (_lookingAt == null) return;

            interactionIndicator.Hide();
            _lookingAt.Interact(Player, null); /// IInteractArgs is undefined as of now
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

        void OnInputPause(InputAction.CallbackContext context)
        {
            /// Pause only when no other windows are visible
            if (Game.UI.HasActiveWindow) return;

            if (Game.World.IsPaused)
                UnpauseWorld();
            else
                PauseWorld();
        }

        #endregion

        void PauseWorld()
        {
            Game.World.Pause();
            SetPlayerPaused(true);
        }

        void UnpauseWorld()
        {
            Game.World.Unpause();
            SetPlayerPaused(false);
        }

        #region Public methods

        /// <summary>
        /// Pick up an Item, or from an ItemEntity and put in the inventory.
        /// </summary>
        /// <returns>Whether or not had successfully picked up the Item.</returns>
        public bool PickUpItem(Item item)
        {
            if (!Player.CanPickUpItems) return false;
            if (item == null || item.IsNone) return false;

            if (item.Data.Type == ItemType.Weapon) /// put in equipment (if possible)
            {
                if (Player.Inventory.Equipment.TryEquipWeapon(item, out EquipmentIndex index))
                {
                    Player.FPP.HoldItem(item.Data);
                    // if (itemEntity is GunItemEntity gun)
                    // {
                    //     Player.FPP.HeldItem
                    // }
                    OnPickupItem?.Invoke(item);
                    return true;
                }
            }
            else if (item.Data.Type == ItemType.Tool) /// put in hotbar (if possible)
            {
                if (Player.Inventory.Hotbar.TryPutNearest(item))
                {
                    Player.FPP.HoldItem(item.Data);
                    OnPickupItem?.Invoke(item);
                    return true;
                }
            }
            
            /// Store to Bag generic items or items can't hold
            if (Player.Inventory.IsFull)
            {
                Game.Console.Log($"Can't pick up '{item.Id}'. Inventory is full");
                return false;
            }
            if (Player.Inventory.Bag.TryPutNearest(item))
            {
                OnPickupItem?.Invoke(item);
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Pick up an Item, or from an ItemEntity and put in the inventory.
        /// </summary>
        /// <returns>Whether or not had successfully picked up the Item.</returns>
        public bool PickUpItem(ItemEntity itemEntity)
        {
            if (!Player.CanPickUpItems) return false;
            if (Player.FPP.IsPerforming) return false;

            Item item = itemEntity.Item;
            if (PickUpItem(item))
            {
                _lookingAt = null;
                return true;
            }
            else
            {
                return false;
            }
        }

        public void EnterVehicle(VehicleEntity vehicle)
        {
            IsBusy = true;
            Player.MoveStateMachine.ToState(MoveStates.InVehicle, lockForSeconds: 9999999f);
            Player.Rigidbody.constraints = RigidbodyConstraints.None;
            Vehicle = vehicle;
            EnableVehicleControls(vehicle);

            OnInteractVehicle?.Invoke(new()
            {
                Interactable = vehicle,
                Actor = Player,
                Entered = true
            });
        }

        public void ExitVehicle(VehicleEntity vehicle)
        {
            DisableVehicleControls(vehicle);
            vehicle.SeatManager.ExitVehicle(Player);
            Vehicle = null;
            Player.MoveStateMachine.Unlock();
            Player.MoveStateMachine.ToState(MoveStates.Idle);
            Player.Rigidbody.constraints = _savedRigidbodyConstraints;
            IsBusy = false;

            OnInteractVehicle?.Invoke(new()
            {
                Interactable = vehicle,
                Actor = Player,
                Exited = true
            });
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

        /// <summary>
        /// Disables Player actions. This include:
        /// - Picking up items
        /// - Interacting with pickups/objects
        /// - Equipping items via hotbar
        /// - etc. idk
        /// </summary>
        public void Disable()
        {
            actionMap.Disable();
            _allowInteractions = false;
            
            interactionIndicator.Hide();
            _lookingAt?.OnLookExit();
            _lookingAt = null;
        }

        public void SetControl(string name, bool enable)
        {
            if (inputs.ContainsKey(name))
            {
                if (enable)
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


        #region Vehicle input callbacks

        /// <summary>
        /// The vehicle this Player is currently on (if any).
        /// </summary>
        public VehicleEntity Vehicle { get; protected set; }        

        void EnableVehicleControls(VehicleEntity vehicle)
        {
            Inputs["Change Seat"].performed += OnInputChangeSeat;
            Inputs["Change Vehicle View"].performed += OnInputChangeVehicleView;
            backInput.performed += ExitVehicleOnGlobalBack;
        }

        void DisableVehicleControls(VehicleEntity vehicle)
        {
            Inputs["Change Seat"].performed -= OnInputChangeSeat;
            Inputs["Change Vehicle View"].performed -= OnInputChangeVehicleView;
            backInput.performed -= ExitVehicleOnGlobalBack;
        }

        void OnInputChangeSeat(InputAction.CallbackContext context)
        {
            Vehicle.SeatManager.ChangeSeat(Player);
        }

        void OnInputChangeVehicleView(InputAction.CallbackContext context)
        {
            Vehicle.CameraManager.ChangeVehicleView(Player);
        }

        void ExitVehicleOnGlobalBack(InputAction.CallbackContext context)
        {
            ExitVehicle(Vehicle);
        }

        #endregion

        
        /// <summary>
        /// Visualizes the interaction size.
        /// </summary>
        void OnDrawGizmos()
        {
            Gizmos.DrawLine(_ray.origin, _ray.origin + _ray.direction * (MaxInteractDistance));
            Gizmos.DrawWireSphere(_ray.origin + _ray.direction * (MaxInteractDistance + Radius), Radius);
        }
    }
}
