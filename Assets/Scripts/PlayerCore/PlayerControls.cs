using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.InputSystem;

using UZSG.Systems;
using UZSG.Entities;
using UZSG.Attributes;

namespace UZSG.Players
{
    public struct FrameInput
    {
        public Vector2 Move { get; set; }
        public bool HasPressedJump { get; set; }
        public bool HasPressedInteract { get; set; }
        public bool HasPressedInventory { get; set; }
    }

    /// <summary>
    /// Controls the player movement.
    /// </summary>
    public class PlayerControls : MonoBehaviour
    {
        public Player Player;
        [Space]

        public bool EnableMovementControls = true;


        #region These should be read as attributes
        /// <summary>
        /// Cached movement values from the Player. This should subscribe to the OnValueChanged event.
        /// </summary>
        public float Acceleration;
        public float MoveSpeed;
        public float RunSpeed;
        public float CrouchSpeed;
        public float JumpSpeed = 2f;
        public float TimeScale = 1f;

        #endregion

        bool _isMovePressed;
        bool _isMovingBackwards;
        bool _isTransitioningCrouch;
        bool _isRunning;
        [SerializeField] float _targetSpeed;
        float _crouchPosition;
        /// <summary>
        /// The input values performed in the current frame.
        /// </summary>
        FrameInput _frameInput;
        /// <summary>
        /// The velocity to be applied for the current frame.
        /// </summary>
        Vector3 _frameVelocity;

        /// <summary>
        /// If holding [Run] key and speed is greater than run threshold.
        /// </summary>
        public bool IsRunning => _isRunning;
        bool _isCrouching;
        public bool IsCrouching => _isCrouching;
        public bool IsMoving
        {
            get => Magnitude > 0f;
        }
        public bool IsGrounded => groundChecker.IsGrounded;
        public bool IsFalling
        {
            get => _frameVelocity.y < 0f && !IsGrounded;
        }
        public Vector3 Velocity
        {
            get => rb.velocity;
        }
        public float Magnitude
        {
            get => rb.velocity.magnitude;   
        }
        /// <summary>
        /// Represents how fast the player is moving in any direction.
        /// </summary>
        public float Speed
        {
            get => rb.velocity.magnitude;
        }
        public float HorizontalSpeed
        {
            get => new Vector3(rb.velocity.x, 0f, rb.velocity.z).magnitude;
        }
        public float VerticalSpeed
        {
            get => new Vector3(0f, rb.velocity.y, 0f).magnitude;
        }
        public bool CanBob
        {
            get => IsGrounded;
        }

        #region Events

        public event EventHandler OnMoveStart;
        public event EventHandler OnMoveStop;

        #endregion



        [Header("Components")]
        /// <summary>
        /// The Player's 3D model.
        /// </summary>
        [SerializeField] Transform model;
        [SerializeField] Rigidbody rb;
        public Rigidbody Rigidbody => rb;
        public GroundChecker groundChecker;
        [SerializeField] Transform groundCheckerObject;
        [SerializeField] LayerMask groundMask;
        
        InputActionMap actionMap;
        public InputActionMap ActionMap => actionMap;
        Dictionary<string, InputAction> inputs = new();
        public Dictionary<string, InputAction> Inputs => inputs;
        
        internal void Initialize()
        {
            InitializeInputs();
            RetrieveAttributes();
            
            Game.Tick.OnTick += Tick;
        }

        void InitializeInputs()
        {
            actionMap = Game.Main.GetActionMap("Player Move");
            inputs = Game.Main.GetActionsFromMap(actionMap);

            foreach (var input in inputs)
            {
                input.Value.performed += OnPerformInput;
            }
            
            inputs["Move"].performed += OnMoveInput;
            inputs["Move"].started += OnMoveInput;
            inputs["Move"].canceled += OnMoveInput;

            inputs["Jump"].started += OnJumpInput;          // Space (default)
            inputs["Jump"].canceled += OnJumpInput;        

            inputs["Run"].started += OnRunInput;            // Shift (default)
            inputs["Run"].canceled += OnRunInput;           

            inputs["Crouch"].started += OnCrouchInput;      // LCtrl (default)
            inputs["Crouch"].canceled += OnCrouchInput;   

            SetControlsEnabled(true);
        }

        void OnPerformInput(InputAction.CallbackContext context)
        {
            OnInput?.Invoke(context);
        }

        public event Action<InputAction.CallbackContext> OnInput; 
        
        void Awake()
        {
            Player = GetComponent<Player>();
        }

        void FixedUpdate()
        {
            HandleMovement();
            UpdateStates();
        }

        void Tick(TickInfo t)
        {
        }

        void HandleMovement()
        {
            HandleDirection();
            HandleRotation();
            ApplyMovement();
        }

        void RetrieveAttributes()
        {
            /// These can be cached and track changes using events
            /// Vitals
            stamina = Player.Vitals.Get("stamina");
            
            /// Generic
            MoveSpeed = Player.Generic.Get("move_speed").Value;
            RunSpeed = Player.Generic.Get("run_speed").Value;
            CrouchSpeed = Player.Generic.Get("crouch_speed").Value;
            jumpStaminaCost = Player.Generic.Get("jump_stamina_cost").Value;
            
            _targetSpeed = MoveSpeed;
        }

        void UpdateStates()
        {
            if (_isMovePressed)
            {
                if (IsRunning)
                {
                    Player.MoveStateMachine.ToState(MoveStates.Run);
                }
                else
                {
                    Player.MoveStateMachine.ToState(MoveStates.Walk);
                }
            }
            else
            {
                Player.MoveStateMachine.ToState(MoveStates.Idle);
            }
        }


        #region Cached attributes

        Attributes.Attribute stamina;
        float jumpStaminaCost;

        #endregion


        #region Player input callbacks

        void OnMoveInput(InputAction.CallbackContext context)
        {
            if (!EnableMovementControls) return;

            _frameInput.Move = context.ReadValue<Vector2>();
            _isMovePressed = _frameInput.Move.x != 0 || _frameInput.Move.y != 0;
            _isMovingBackwards = _frameInput.Move.y < 0;

            CancelRunIfRunningBackwards();
        }

        void CancelRunIfRunningBackwards()
        {
            if (_isMovingBackwards && _isRunning)
            {
                ToggleRun(false);
            }
        }

        void OnRunInput(InputAction.CallbackContext context)
        {
            if (!EnableMovementControls) return;
            if (!IsGrounded) return;

            if (context.started)
            {
                CancelRunBecauseOfOtherThings();
                if (Player.FPP.IsPerforming) return;

                ToggleRun(true);
            }
            else if (context.canceled)
            {
                ToggleRun(false);
            }
        }

        void CancelRunBecauseOfOtherThings()
        {
            if (_isMovingBackwards)
            {
                ToggleRun(false);
            }
        }

        void OnJumpInput(InputAction.CallbackContext context)
        {
            if (!EnableMovementControls) return;

            if (context.started)
            {
                if (!Player.CanJump) return;
            
                if (_isRunning)
                {
                    ToggleRun(false);
                }
                if (_isCrouching)
                {
                    ToggleCrouch(false);
                }
                
                HandleJump();
            }
        }

        [SerializeField] bool _crouchControlIsToggle = true;

        void OnCrouchInput(InputAction.CallbackContext context)
        {
            if (!EnableMovementControls) return;

            if (_crouchControlIsToggle)
            {
                if (context.started)
                {
                    ToggleCrouch(!_isCrouching);
                }
            }
            else // is hold
            {
                if (context.started)
                {
                    ToggleCrouch(true);
                }
                else if (context.canceled)
                {
                    ToggleCrouch(false);
                }
            }
        }

        #endregion


        Vector3 GetCameraForward()
        {
            Vector3 camForward = Player.MainCamera.transform.forward;
            camForward.y = 0f;
            return camForward;
        }

        void ToggleRun(bool run)
        {
            if (run)
            {
                if (_isCrouching)
                {
                    ToggleCrouch(false);
                }

                _targetSpeed = RunSpeed;
                Player.MoveStateMachine.ToState(MoveStates.Run);
            }
            else
            {
                _targetSpeed = MoveSpeed;
                Player.MoveStateMachine.ToState(MoveStates.Idle);
            }
            _isRunning = run;
        }

        void HandleJump()
        {
            if (!IsGrounded) return;
            
            Vector3 jumpVelocity = rb.velocity;
            jumpVelocity.y = JumpSpeed * TimeScale;
            rb.velocity = jumpVelocity;
            Player.MoveStateMachine.ToState(MoveStates.Jump);
        }

        void HandleDirection()
        {
            _frameVelocity = (_frameInput.Move.x * Player.MainCamera.transform.right) + (_frameInput.Move.y * GetCameraForward().normalized);
            _frameVelocity.Normalize();
        }

        void HandleRotation()
        {
            model.rotation = Quaternion.LookRotation(GetCameraForward().normalized);
        }

        void ApplyMovement()
        {
            var targetVelocity = _frameVelocity * (_targetSpeed * Time.fixedDeltaTime);
            targetVelocity.y = rb.velocity.y;
            /// No acceleration
            // rb.velocity = targetVelocity * TimeScale;
            /// With acceleration
            rb.velocity = Vector3.Lerp(rb.velocity, targetVelocity, Acceleration * Time.fixedDeltaTime) * TimeScale;
        }

        void ToggleCrouch(bool crouch)
        {
            if (_isTransitioningCrouch) return;
            _isTransitioningCrouch = true;

            float transitionSpeed = 0.66f;
            Player.MoveStateMachine.ToState(MoveStates.Crouch);
            
            if (crouch)
            {
                Player.HUD.vignette.CrossFadeAlpha(1f, 0.5f, false);
                _targetSpeed = CrouchSpeed;
                _crouchPosition = Player.FPP.Camera.transform.position.y - 1f;
            }
            else
            {
                Player.HUD.vignette.CrossFadeAlpha(0f, 0.5f, false);
                _targetSpeed = MoveSpeed;
                _crouchPosition = Player.FPP.Camera.transform.position.y + 1f;
            }
            _isCrouching = crouch;

            LeanTween.value(gameObject, Player.FPP.Camera.transform.position.y, _crouchPosition, transitionSpeed)
            .setOnUpdate((float i) =>
            {   
                Player.FPP.Camera.transform.position = new Vector3(
                    Player.FPP.Camera.transform.position.x,
                    i,
                    Player.FPP.Camera.transform.position.z
                );
            })
            .setOnComplete(() =>
            {
                _isTransitioningCrouch = false;
            }).setEaseOutExpo();
        }

        public void Enable()
        {
            actionMap.Enable();
        }

        public void Disable()
        {
            actionMap.Disable();
        }

        void SetControlsEnabled(bool value)
        {
            if (value)
            {
                actionMap.Enable();
            }
            else
            {
                actionMap.Disable();
            }
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
    }
}