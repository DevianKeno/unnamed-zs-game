using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Cinemachine;
using UZSG.Systems;
using UZSG.FPP;
using UZSG.Entities;

namespace UZSG.PlayerCore
{
    public struct FrameInput
    {
        public Vector2 move;
        public bool pressedJump;
        public bool pressedInteract;
        public bool pressedInventory;
    }

    /// <summary>
    /// Controls the player movement.
    /// </summary>
    public class PlayerControls : MonoBehaviour
    {
        #region These should be read as attributes
        /// <summary>
        /// Cached movement values from the Player. This should subscribe to the OnValueChanged event.
        /// </summary>
        public float MoveSpeed;
        public float RunSpeed;
        public float CrouchSpeed;
        public float JumpTime = 1f;
        public float JumpHeight = 2f;
        public float Gravity = -9.81f;
        public float FallSpeedMultiplier = 2f;
        public float CrouchSpeedMultiplier = 0.7f;
        public float LookRotationSpeed = 100.0f;
        #endregion
        
        public float GroundingForce = -1f;
        public float GroundDistance = 0.5f;

        [Header("Controls")]
        public float CameraSensitivity = 0.32f;
        public bool EnableMovementControls = true;
        public bool EnableCameraControls = true;
        /// <summary>
        /// The input values performed in the current frame.
        /// </summary>
        FrameInput _frameInput;
        /// <summary>
        /// The velocity to be applied for the current frame.
        /// </summary>
        Vector3 _frameVelocity;
        Vector3 _previousPosition;
        Vector3 _targetPosition;

        float _currentSpeed;
        bool _isMovePressed;
        float initialJumpVelocity;
        bool _jumped;
        float CrouchPosition;
        bool _isTransitioning;
        bool _isRunning;
        /// <summary>
        /// Check if holding [Run] key and speed is greater than run threshold
        /// </summary>
        public bool IsRunning { get => _isRunning; }
        bool _isCrouching;
        public bool isCrouching { get => _isCrouching; }

        public bool IsMoving
        {
            get
            {
                if (Magnitude > 0) return true;
                return false;
            }
        }

        public bool IsGrounded
        {
            get
            {
                return Physics.Raycast(groundChecker.position, Vector3.down, GroundDistance, groundMask);
            }
        }

        public bool IsFalling
        {
            get => _frameVelocity.y <= 0f && !IsGrounded;
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

        public Player Player { get => player; }

        [Header("Components")]
        [SerializeField] Player player;
        /// <summary>
        /// The Player's 3D model.
        /// </summary>
        [SerializeField] Transform model;
        [SerializeField] Rigidbody rb;
        [SerializeField] Transform groundChecker;
        [SerializeField] LayerMask groundMask;
        [SerializeField] PlayerInput input;
        InputAction moveInput;
        InputAction jumpInput;
        InputAction runInput;
        InputAction crouchInput;

        void Awake()
        {
            player = GetComponent<Player>();
            input = GetComponent<PlayerInput>();
            
            moveInput = input.actions.FindAction("Move");
            jumpInput = input.actions.FindAction("Jump");
            runInput = input.actions.FindAction("Run");
            crouchInput = input.actions.FindAction("Crouch");
        }

        // void OnDrawGizmos()
        // {            
        //     Gizmos.DrawLine(groundChecker.transform.position, groundChecker.transform.position + Vector3.down * GroundDistance);
        // }

        internal void Init()
        {
            InitializeControls();
            RetrieveAttributes();
        }

        void InitializeControls()
        {            
            SetControlsEnabled(true);

            moveInput.performed += OnMoveInput;
            moveInput.started += OnMoveInput;
            moveInput.canceled += OnMoveInput;

            jumpInput.performed += OnJumpInput;                 // Space (default)

            runInput.started += OnRunInput;                     // Shift (default)  

            runInput.canceled += OnRunInput;                    // Shift

            crouchInput.performed += OnCrouchInput;             // LCtrl (default)

            Game.UI.ConsoleUI.OnToggle += ConsoleWindowToggledCallback;
            Game.Tick.OnTick += Tick;
            _previousPosition = transform.position;
        }

        void Tick(object sender, TickEventArgs e)
        {
            if (IsMoving && IsRunning)
            {
                // Cache attributes for better performance
                var runStaminaCost = player.Generic.GetAttributeFromId("run_stamina_cost").Value;
                player.Vitals.GetAttributeFromId("stamina").Remove(runStaminaCost);
            }
        }

        void ConsoleWindowToggledCallback(bool value)
        {
            SetControlsEnabled(!value);
        }

        void SetControlsEnabled(bool value)
        {
            if (value)
            {
                moveInput.Enable();
                jumpInput.Enable();
                runInput.Enable();
                crouchInput.Enable();
            } else
            {
                moveInput.Disable();
                jumpInput.Disable();
                runInput.Disable();
                crouchInput.Disable();
            }
        }

        void OnDisable()
        {
            SetControlsEnabled(false);
        }

        void RetrieveAttributes()
        {
            // These can be cached and track changes using events
            MoveSpeed = player.Generic.GetAttributeFromId("move_speed").Value;
            RunSpeed = player.Generic.GetAttributeFromId("run_speed").Value;
            CrouchSpeed = player.Generic.GetAttributeFromId("crouch_speed").Value;
            
            _currentSpeed = MoveSpeed;
        }

        void FixedUpdate()
        {
            HandleMovement();
            CheckState();
        }

        void CheckState()
        {
            if (_isMovePressed)
            {
                if (IsRunning)
                {
                    player.smMove.ToState(MoveStates.Run);
                } else
                {
                    player.smMove.ToState(MoveStates.Walk);
                }
            } else
            {
                player.smMove.ToState(MoveStates.Idle);
            }
        }

        void OnMoveInput(InputAction.CallbackContext context)
        {
            if (!EnableMovementControls) return;

            _frameInput.move = context.ReadValue<Vector2>();
            _isMovePressed = _frameInput.move.x != 0 || _frameInput.move.y != 0;
        }

        void OnJumpInput(InputAction.CallbackContext context)
        {
            if (!EnableMovementControls) return;

            if (IsGrounded)
            {
                _jumped = true;
            }
        }

        void OnRunInput(InputAction.CallbackContext context)
        {            
            if (!EnableMovementControls) return;

            if (_isCrouching)
            {
                Crouch();
            }

            ToggleRun(!_isRunning);
        }

        void ToggleRun(bool value)
        {
            _isRunning = value;
            if (value)
            {
                _currentSpeed = RunSpeed;
                player.smMove.ToState(MoveStates.Run);
            } else
            {
                _currentSpeed = MoveSpeed;
                player.smMove.ToState(MoveStates.Idle);
            }
        }

        void OnCrouchInput(InputAction.CallbackContext context)
        {            
            if (!EnableMovementControls) return;

            Crouch();
        }

        Vector3 GetCameraForward()
        {
            Vector3 camForward = player.MainCamera.transform.forward;
            camForward.y = 0f;
            return camForward;
        }

        void HandleMovement()
        {
            HandleDirection();
            HandleRotation();
            HandleGravity();
            HandleJump();

            ApplyMovement();
        }

        void HandleDirection()
        {
            _frameVelocity = (_frameInput.move.x * player.MainCamera.transform.right) + (_frameInput.move.y * GetCameraForward().normalized);
            _frameVelocity.Normalize();
        }

        void HandleRotation()
        {
            model.rotation = Quaternion.LookRotation(GetCameraForward().normalized);
        }

        void HandleJump()
        {
            if (!_jumped) return;
            _jumped = false;
            
            player.smMove.ToState(MoveStates.Jump);
            // The time required to reach the highest point of the jump
            float timeToApex = JumpTime / 2;
            _frameVelocity.y = (2 * JumpHeight) / timeToApex; // this should be cached
        }

        void HandleGravity()
        {            
            // Calculates the player's internal gravity
            Gravity = (-2 * JumpHeight) / Mathf.Pow(JumpTime / 2, 2); // this should be cached

            if (IsGrounded) // grounding force only
            {
                _frameVelocity.y = GroundingForce;

            } else if (IsFalling) // increasing fall speed
            {
                _frameVelocity.y += Gravity * FallSpeedMultiplier * Time.fixedDeltaTime;
            } else // normal gravity
            {
                _frameVelocity.y += Gravity * Time.fixedDeltaTime;
            }
        }
        
        void ApplyMovement()
        {
            _targetPosition = _previousPosition + (_frameVelocity * (_currentSpeed * Time.fixedDeltaTime));

            if (_previousPosition != _targetPosition)
            {
                Vector3 displacement = _targetPosition - _previousPosition;
                rb.velocity = displacement / Game.Tick.SecondsPerTick; // Will break if SecondsPerTick is 0
            } else
            {
                rb.velocity = Vector3.zero;
            }
        }

        void Crouch()
        {
            if (_isTransitioning) return;

            player.smMove.ToState(MoveStates.Crouch);

            _isTransitioning = !_isTransitioning;
            _isCrouching = !_isCrouching;

            float TransitionSpeed;
            
            if (_isCrouching)
            {            
                _currentSpeed = CrouchSpeed;
                CrouchPosition = player.FPP.Camera.transform.position.y - 1f;
                TransitionSpeed = 0.3f;
            } else
            {
                _currentSpeed = MoveSpeed;
                CrouchPosition = player.FPP.Camera.transform.position.y + 1f;
                TransitionSpeed = 0.3f;
            }

            LeanTween.value(gameObject, player.FPP.Camera.transform.position.y, CrouchPosition, TransitionSpeed)
                .setOnUpdate( (i) =>
                {   
                    player.FPP.Camera.transform.position = new Vector3(
                        player.FPP.Camera.transform.position.x,
                        i,
                        player.FPP.Camera.transform.position.z
                    );
                }).setOnComplete( () =>
                {
                    _isTransitioning = false;
                }).setEaseOutExpo();
        }

        public void ToggleMovementControls()
        {
            ToggleMovementControls(!EnableMovementControls);
        }
        
        public void ToggleMovementControls(bool value)
        {
            EnableMovementControls = value;
            if (value)
            {
                moveInput.Enable();
            } else
            {
                moveInput.Disable();
            }
        }
    }
}