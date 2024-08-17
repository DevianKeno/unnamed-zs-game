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

        public bool AllowMovement;
        public float TimeScale = 1f;

        bool _isMovePressed;
        bool _isMovingBackwards;
        bool _isTransitioningCrouch;
        bool _isWalking;
        bool _isRunning;
        bool _hasJumped;
        [SerializeField] bool _runIsToggle = false;
        [SerializeField] bool _crouchIsToggle = true;

        [SerializeField] float acceleration = 10f;
        float _walkSpeed;
        float _moveSpeed;
        float _runSpeed;
        float _crouchSpeed;
        float _jumpSpeed = 2f;
        float _targetSpeed;
        float _crouchPosition;
        /// <summary>
        /// The input values performed in the current frame.
        /// </summary>
        FrameInput _frameInput;
        /// <summary>
        /// The input values performed in the current frame.
        /// </summary>
        public FrameInput FrameInput => _frameInput;
        /// <summary>
        /// The velocity to be applied for the current frame.
        /// </summary>
        Vector3 _frameVelocity;
        MoveStates _targetMoveState;


        #region Public properties

        public bool IsWalking => _isWalking;
        /// <summary>
        /// If holding [Run] key and speed is greater than run threshold.
        /// </summary>
        public bool IsRunning => _isRunning;
        bool _isCrouching;
        public bool IsCrouching => _isCrouching;
        public bool IsMoving
        {
            get => _frameInput.Move != Vector2.zero;
        }
        public bool IsGrounded
        {
            get => groundChecker.IsGrounded;
        }
        public bool IsFalling
        {
            get => _frameVelocity.y < 0f && !IsGrounded;
        }
        public Vector3 Velocity
        {
            get => rb.velocity;
        }
        public Vector3 LocalVelocity
        {
            get => Player.MainCamera.transform.InverseTransformDirection(Velocity);
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
        Vector3 CameraForward
        {
            get
            {
                Vector3 camForward = Player.MainCamera.transform.forward;
                camForward.y = 0f;
                return camForward;
            }
        }

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
        
        
        #region Initializing methods

        void Awake()
        {
            Player = GetComponent<Player>();
        }
        
        internal void Initialize()
        {
            InitializeInputs();
            RetrieveAttributes();
        }

        void InitializeInputs()
        {
            actionMap = Game.Main.GetActionMap("Player Move");
            inputs = Game.Main.GetActionsFromMap(actionMap);
            
            inputs["Move"].performed += OnInputMove;
            inputs["Move"].started += OnInputMove;
            inputs["Move"].canceled += OnInputMove;

            inputs["Jump"].started += OnInputJump;
            inputs["Jump"].canceled += OnInputJump;        

            inputs["Run"].started += OnInputRun;
            inputs["Run"].canceled += OnInputRun;           

            inputs["Toggle Walk"].performed += OnInputWalkToggle;

            inputs["Crouch"].started += OnInputCrouch;
            inputs["Crouch"].canceled += OnInputCrouch;

            Enable();
        }

        void RetrieveAttributes()
        {
            _walkSpeed = Player.Attributes.Get("walk_speed").Value;
            _moveSpeed = Player.Attributes.Get("move_speed").Value;
            _runSpeed = Player.Attributes.Get("run_speed").Value;
            _crouchSpeed = Player.Attributes.Get("crouch_speed").Value;
            
            _targetSpeed = _moveSpeed;
        }

        #endregion


        void FixedUpdate()
        {
            HandleMovement();
            UpdateStates();
        }

        void HandleMovement()
        {
            HandleDirection();
            HandleRotation();
            ApplyMovement();
        }

        void UpdateStates()
        {
            if (_isMovePressed && IsMoving && !_hasJumped)
            {
                Player.MoveStateMachine.ToState(_targetMoveState);
            }
            else
            {
                Player.MoveStateMachine.ToState(MoveStates.Idle);
            }

            if (_hasJumped)
            {
                if (IsGrounded)
                {
                    _hasJumped = false;
                }
            }
        }


        #region Player input callbacks

        void OnInputMove(InputAction.CallbackContext context)
        {
            if (!AllowMovement) return;

            _frameInput.Move = context.ReadValue<Vector2>();
            _isMovePressed = _frameInput.Move.x != 0 || _frameInput.Move.y != 0;
            _isMovingBackwards = _frameInput.Move.y < 0;

            CancelRunIfRunningBackwards();
        }

        void OnInputRun(InputAction.CallbackContext context)
        {
            if (!AllowMovement) return;
            if (!IsGrounded) return;

            if (context.started)
            {
                CancelRunBecauseOfOtherThings();
                if (Player.FPP.IsPerforming) return;
                if (IsWalking)
                {
                    ToggleWalk(false);
                }

                ToggleRun(true);
            }
            else if (context.canceled)
            {
                ToggleRun(false);
            }
        }

        void OnInputWalkToggle(InputAction.CallbackContext context)
        {
            if (IsRunning)
            {
                ToggleRun(false);
            }

            ToggleWalk(!IsWalking);
        }

        void OnInputJump(InputAction.CallbackContext context)
        {
            if (!AllowMovement) return;

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

        void OnInputCrouch(InputAction.CallbackContext context)
        {
            if (!AllowMovement) return;

            if (_crouchIsToggle)
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


        void CancelRunIfRunningBackwards()
        {
            if (_isMovingBackwards && _isRunning)
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

        void ToggleRun(bool run)
        {
            if (run)
            {
                if (_isCrouching)
                {
                    ToggleCrouch(false);
                }

                _targetSpeed = _runSpeed;
                _targetMoveState = MoveStates.Run;
            }
            else
            {
                _targetSpeed = _moveSpeed;
                _targetMoveState = default;
            }

            _isRunning = run;
        }
        

        void ToggleWalk(bool walk)
        {
            if (walk)
            {
                _targetSpeed = _walkSpeed;
                _targetMoveState = MoveStates.Walk;
            }
            else
            {
                _targetSpeed = _moveSpeed;
                _targetMoveState = default;
            }

            _isWalking = walk;
        }

        void HandleJump()
        {
            if (!IsGrounded) return;

            _hasJumped = true;
            Vector3 jumpVelocity = rb.velocity;
            jumpVelocity.y = _jumpSpeed * TimeScale;
            rb.velocity = jumpVelocity;
            Player.MoveStateMachine.ToState(MoveStates.Jump);
        }

        void HandleDirection()
        {
            _frameVelocity = (_frameInput.Move.x * Player.MainCamera.transform.right) + (_frameInput.Move.y * CameraForward.normalized);
            _frameVelocity.Normalize();
        }

        void HandleRotation()
        {
            model.rotation = Quaternion.LookRotation(CameraForward.normalized);
        }

        void ApplyMovement()
        {
            var targetVelocity = _frameVelocity * (_targetSpeed * Time.fixedDeltaTime);
            targetVelocity.y = rb.velocity.y;
            /// No acceleration
            // rb.velocity = targetVelocity * TimeScale;
            /// With acceleration
            rb.velocity = Vector3.Lerp(rb.velocity, targetVelocity, acceleration * Time.fixedDeltaTime) * TimeScale;
        }

        void ToggleCrouch(bool crouch)
        {
            if (_isTransitioningCrouch) return;
            _isTransitioningCrouch = true;

            float transitionSpeed = 0.66f;
            Player.MoveStateMachine.ToState(MoveStates.Crouch);
            
            if (crouch)
            {
                _targetSpeed = _crouchSpeed;
                _crouchPosition = Player.FPP.Camera.transform.position.y - 1f;
                Player.InfoHUD.FadeVignette(1f);
            }
            else
            {
                _targetSpeed = _moveSpeed;
                _crouchPosition = Player.FPP.Camera.transform.position.y + 1f;
                Player.InfoHUD.FadeVignette(0f);
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


        #region Public methods

        public void Enable()
        {
            actionMap.Enable();
            AllowMovement = true;
        }

        public void Disable()
        {
            actionMap.Disable();
            AllowMovement = false;
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
    }
}