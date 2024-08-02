using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.InputSystem;

using UZSG.Systems;
using UZSG.Entities;

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

        bool _isEnabled;
        /// <summary>
        /// The input values performed in the current frame.
        /// </summary>
        FrameInput _frameInput;
        /// <summary>
        /// The velocity to be applied for the current frame.
        /// </summary>
        Vector3 _frameVelocity;

        StrafeDirection strafeDirection;
        public StrafeDirection StrafeDirection => strafeDirection;
        [SerializeField] float _targetSpeed;
        bool _isMovePressed;
        bool _isMovingBackwards;
        float _crouchPosition;
        bool _isTransitioningCrouch;
        bool _isRunning;
        /// <summary>
        /// Check if holding [Run] key and speed is greater than run threshold
        /// </summary>
        public bool IsRunning { get => _isRunning; }
        bool _isCrouching;
        public bool IsCrouching { get => _isCrouching; }
        public bool IsMoving
        {
            get
            {
                if (Magnitude > 0) return true;
                return false;
            }
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
        /// <summary>
        /// Represents how fast the player is moving in any direction.
        /// </summary>
        public float Speed
        {
            get => rb.velocity.magnitude;
        }
        public float HorizontalSpeed
        {
            get
            {
                var horizontalVelocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
                return horizontalVelocity.magnitude;
            }
        }
        public float VerticalSpeed
        {
            get => rb.velocity.y;
        }
        public float Magnitude
        {
            get => new Vector3(rb.velocity.x, 0, rb.velocity.z).magnitude;   
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
        [SerializeField] GroundChecker groundChecker;
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
            actionMap = Game.Main.GetActionMap("Player");
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

        void Tick(TickInfo e)
        {
            if (IsMoving && IsRunning)
            {
                /// Cache attributes for better performance
                var runStaminaCost = Player.Generic.GetAttribute("run_stamina_cost").Value;
                Player.Vitals.GetAttribute("stamina").Remove(runStaminaCost);
            }
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
            MoveSpeed = Player.Generic.GetAttribute("move_speed").Value;
            RunSpeed = Player.Generic.GetAttribute("run_speed").Value;
            CrouchSpeed = Player.Generic.GetAttribute("crouch_speed").Value;
            
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


        #region Player input callbacks

        void OnMoveInput(InputAction.CallbackContext context)
        {
            if (!_isEnabled) return;
            if (!EnableMovementControls) return;

            _frameInput.Move = context.ReadValue<Vector2>();
            _isMovePressed = _frameInput.Move.x != 0 || _frameInput.Move.y != 0;
            _isMovingBackwards = _frameInput.Move.y < 0;

            if (_frameInput.Move.x < 0)
            {
                strafeDirection = StrafeDirection.Left;
            }
            else if (_frameInput.Move.x > 0)
            {
                strafeDirection = StrafeDirection.Right;
            }
            else
            {
                strafeDirection = StrafeDirection.None;
            }
        }

        void OnRunInput(InputAction.CallbackContext context)
        {
            if (!_isEnabled) return;
            if (!EnableMovementControls) return;
            if (!IsGrounded) return;

            if (context.started)
            {
                if (_isMovingBackwards)
                {
                    ToggleRun(false);
                    return;
                }
                if (Player.FPP.IsPerforming) return;

                ToggleRun(true);
            }
            else if (context.canceled)
            {
                ToggleRun(false);
            }
        }

        void OnJumpInput(InputAction.CallbackContext context)
        {
            if (!_isEnabled) return;
            if (!EnableMovementControls) return;

            if (context.started)
            {
                if (_isRunning)
                {
                    ToggleRun(false);
                }
                HandleJump();
            }
        }

        public bool CanBob
        {
            get
            {
                if (IsGrounded) return true;
                return false;
            }
        }

        [SerializeField] bool _crouchControlIsToggle = true;

        void OnCrouchInput(InputAction.CallbackContext context)
        {
            if (!_isEnabled) return;
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
            if (_isCrouching)
            {
                ToggleCrouch(false);
            }

            if (run)
            {
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
            rb.velocity = targetVelocity * TimeScale;
        }

        void ToggleCrouch(bool crouch)
        {
            if (_isTransitioningCrouch) return;
            _isTransitioningCrouch = true;

            float transitionSpeed = 0.66f;
            Player.MoveStateMachine.ToState(MoveStates.Crouch);
            
            if (crouch)
            {            
                _targetSpeed = CrouchSpeed;
                _crouchPosition = Player.FPP.CameraController.transform.position.y - 1f;
            }
            else
            {
                _targetSpeed = MoveSpeed;
                _crouchPosition = Player.FPP.CameraController.transform.position.y + 1f;
            }
            _isCrouching = crouch;

            LeanTween.value(gameObject, Player.FPP.CameraController.transform.position.y, _crouchPosition, transitionSpeed)
            .setOnUpdate((float i) =>
            {   
                Player.FPP.CameraController.transform.position = new Vector3(
                    Player.FPP.CameraController.transform.position.x,
                    i,
                    Player.FPP.CameraController.transform.position.z
                );
            })
            .setOnComplete(() =>
            {
                _isTransitioningCrouch = false;
            }).setEaseOutExpo();
        }

        public void Enable()
        {
            _isEnabled = true;
        }

        public void Disable()
        {
            _isEnabled = false;
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