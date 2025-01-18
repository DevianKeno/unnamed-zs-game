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
using UnityEngine.Serialization;

namespace UZSG.Players
{
    public class Tags
    {
        public const string INTERACTABLE = "Interactable";
    }

    /// <summary>
    /// Handles the different actions the Player can do.
    /// </summary>
    public class PlayerActions : MonoBehaviour
    {
        public Player Player { get; private set; }
        [Space]

        [Header("Interaction Settings")]
        [SerializeField, FormerlySerializedAs("Radius")] float InteractRadius;
        [Range(0.1f, 5f), SerializeField] float MaxInteractDistance;
        [SerializeField] LayerMask interactableLayers;
        [SerializeField] float HoldThresholdMilliseconds = 200f;
        
        bool _hadLeftClicked;
        bool _hadRightClicked;
        bool _isHoldingLeftClick;
        bool _isHoldingRightClick;
        bool _allowInteractions = true;
        float _holdTimer;
        Ray _ray;
        /// <summary>
        /// The interactable object the Player is currently looking at.
        /// </summary>
        IInteractable _lookingAt;
        List<InteractAction> _listeningToInteractActions = new();
        RigidbodyConstraints _savedRigidbodyConstraints;

        /// <summary>
        /// Whether if the player is currently interacting with <i>something</i>.
        /// </summary>
        public bool IsBusy { get; protected set; }

        #region Actions events

        /// <summary>
        /// Called everytime the Player equips something in Equipment
        /// </summary>
        public event Action<ItemData, EquipmentIndex> OnEquipEquipment;
        /// <summary>
        /// Called everytime the Player equips something in Hotbar.
        /// <c>int</c> is the index of the item in the Hotbar.
        /// </summary>
        public event Action<ItemData, int> OnEquipHotbar;
        public event Action<Item> OnPickupItem;
        public event Action<IInteractable, List<InteractAction>> OnLookAtSomething;
        /// <summary>
        /// This should be passed with context.
        /// </summary>
        public event Action<InteractionContext> OnInteract;
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
            InitializeInputs();
            
            Game.Tick.OnTick += OnTick;
        }

        void InitializeEvents()
        {
            Game.World.OnExitWorld += Deinitialize;
            this.OnInteract += OnInteractNotifySelf;
        }

        void InitializeInputs()
        {
            actionMap = Game.Main.GetActionMap("Player Actions");
            inputs = Game.Main.GetActionsFromMap(actionMap);
        
            inputs["Primary Action"].started += OnStartPrimaryAction;           // LMB (default)
            inputs["Primary Action"].canceled += OnCancelPrimaryAction;         // LMB (default)

            inputs["Secondary Action"].started += OnStartSecondaryAction;       // RMB (default)
            inputs["Secondary Action"].canceled += OnCancelSecondaryAction;     // RMB (default)
                        
            backInput = Game.Main.GetInputAction("Back", "Global");
        }

        #endregion


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

        void OnTick(TickInfo e)
        {
        }

        const int MAX_INTERACT_RAYCAST_HITS = 10;
        RaycastHit[] _interactRaycastResults = new RaycastHit[MAX_INTERACT_RAYCAST_HITS];

        void InteractionSphereCast()
        {
            if (!_allowInteractions) return;
            if (IsBusy) return;

            var viewportCenter = new Vector2(Screen.width / 2, Screen.height / 2);
            _ray = Player.MainCamera.ScreenPointToRay(viewportCenter);
            _ray.origin += Player.Forward.normalized * 0.5f;
            int hitCount = Physics.SphereCastNonAlloc(_ray, InteractRadius, _interactRaycastResults, MaxInteractDistance, interactableLayers);
            bool newInteractionFound = false;
            
            for (int i = 0; i < hitCount; i++)
            {
                var hit = _interactRaycastResults[i];
                switch (hit.collider.tag)
                {
                    case Tags.INTERACTABLE:
                    {
                        var interactable = hit.collider.GetComponentInParent<IInteractable>();
                        if (interactable != null && interactable.AllowInteractions)
                        {
                            if (interactable == _lookingAt) return;

                            LookAt(interactable);
                            newInteractionFound = true;
                        }
                        break;
                    }
                }
            }

            if (!newInteractionFound)
            {
                LookAt(null);
            }
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
        /// <param name="resource"></param>
        /// <param name="allowMovement">Whether to allow movement during the pickup.</param>
        public void StartPickupRoutine(Objects.ResourcePickup resource, bool allowMovement = false)
        {
            if (resource == null) return;

            IsBusy = true;
            OnInteract?.Invoke(new InteractionContext()
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
                
                if (!allowMovement)
                {
                    inputs["Interact"].performed -= CancelPickupOnMove;   
                    inputs["Interact"].performed += CancelPickupOnMove;   
                }
            }
            else /// instant
            {
                FinishPickupRoutine(resource);
            }

            void CancelPickupOnMove(InputAction.CallbackContext context)
            {
                CancelPickupResource(resource);
            }
        }

        IEnumerator<float> UpdatePickupRoutine(Objects.ResourcePickup resource, float duration)
        {
            while (_pickupTimer < duration)
            {
                _pickupTimer += Time.deltaTime;
                pickupRingUI.Progress = _pickupTimer / duration;
                
                if (Vector3.Distance(_actionPosition, Player.Position) > MaxInteractDistance + (InteractRadius / 2))
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
            pickupRingUI?.Destruct();
            pickupRingUI = null;
            EnableControlsOnPickupResource();
            OnInteract?.Invoke(new InteractionContext()
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
            pickupRingUI?.Destruct();
            pickupRingUI = null;
            EnableControlsOnPickupResource();
            OnInteract?.Invoke(new InteractionContext()
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
            }
            else /// unpause player
            {
                Game.UI.SetCursorVisible(false);
                Player.Controls.Enable();
                Player.FPP.Camera.ToggleControls(true);
                this.Enable();
            }
        }

        #endregion


        #region Event callbacks
        
        void OnInteractNotifySelf(InteractionContext context)
        {
            if (context.Phase == InteractPhase.Started)
            {
                IsBusy = true;
            }
            else if (context.Phase == InteractPhase.Finished || context.Phase == InteractPhase.Canceled)
            {
                IsBusy = false;
            }
        }

        void OnPerformInteractAction(InteractionContext context)
        {
            context.Actor = this.Player;
            context.Phase = InteractPhase.Started;
            context.Interactable.Interact(context);
            OnInteract?.Invoke(context);
            LookAt(null);
        }

        #endregion


        #region Input callbacks
        
        void OnInputInteractPrimary(InputAction.CallbackContext context)
        {
            if (IsBusy && _lookingAt == null) return;

            if (context.performed)
            {
                // _lookingAt?.Interact(this.Player, null); /// IInteractArgs is undefined as of now
            }
        }

        void OnInputInteractSecondary(InputAction.CallbackContext context)
        {
            if (IsBusy && _lookingAt == null) return;

            //
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


        #region Deinitialization
        
        void Deinitialize()
        {
            Game.World.OnExitWorld -= Deinitialize;
            Game.Tick.OnTick -= OnTick;
            this.OnInteract -= OnInteractNotifySelf;
            DeinitializeInputs();
        }

        void DeinitializeInputs()
        {        
            inputs["Primary Action"].started -= OnStartPrimaryAction;
            inputs["Primary Action"].canceled -= OnCancelPrimaryAction;

            inputs["Secondary Action"].started -= OnStartSecondaryAction;
            inputs["Secondary Action"].canceled -= OnCancelSecondaryAction;
            
            // inputs["Pause"].performed -= OnInputPause;
            Disable();
        }

        #endregion


        #region Public methods

        public void LookAt(IInteractable interactable)
        {
            if (interactable != null)
            {
                _lookingAt?.OnLookExit();
                _lookingAt = interactable;
                _lookingAt.OnLookEnter();

                var actions = _lookingAt.GetInteractActions();
                foreach (var interactAction in actions)
                {
                    _listeningToInteractActions.Add(interactAction);
                    interactAction.OnPerformed -= OnPerformInteractAction;
                    interactAction.OnPerformed += OnPerformInteractAction;
                }
                
                OnLookAtSomething?.Invoke(_lookingAt, actions);
            }
            else // interactable == null
            {
                IsBusy = false;
                _lookingAt?.OnLookExit();
                _lookingAt = null;

                foreach (var ia in _listeningToInteractActions)
                {
                    ia.OnPerformed -= OnPerformInteractAction;
                }
                _listeningToInteractActions.Clear();
                
                OnLookAtSomething?.Invoke(null, null);
            }
        }
  
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
                    OnEquipEquipment?.Invoke(item.Data, index);
                    /// TODO: gun item entities store ammunition data, if is reloaded, etc.
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
                if (Player.Inventory.Hotbar.TryEquip(item, out int putOnIndex))
                {
                    Player.FPP.HoldItem(item.Data);
                    OnEquipHotbar?.Invoke(item.Data, putOnIndex);
                    OnPickupItem?.Invoke(item);
                    return true;
                }
            }
            
            /// Store to Bag generic items or items can't hold
            if (Player.Inventory.IsFull)
            {
                Game.Console.LogInfo($"Can't pick up '{item.Id}'. Inventory is full");
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
            Gizmos.DrawWireSphere(_ray.origin + _ray.direction * (MaxInteractDistance + InteractRadius), InteractRadius);
        }
    }
}
