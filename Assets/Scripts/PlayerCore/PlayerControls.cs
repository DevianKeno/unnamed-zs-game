using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.InputSystem;

using UZSG.Systems;
using UZSG.Entities;

namespace UZSG.Players
{
    /// <summary>
    /// Controls the Player movement controls.
    /// </summary>
    public class PlayerControls : MonoBehaviour
    {
        public Player Player { get; private set; }
        [Space]
        public bool AllowMovement;

        [Header("Parameters")]
        public float MoveAcceleration = 10f;
        public float RotationDamping = 6f;
        public float TurningAngle = 120f;
        public bool RunIsToggle = false;
        public bool CrouchIsToggle = true;

        [Header("Jump Parameters")]
        /// <summary>
        /// Whether to use real life jumping physics calculations.
        /// </summary>
        [SerializeField] bool usePhysicsJump = true;
        [SerializeField] float jumpHeight;
        /// <summary>
        /// Total amount of time to complete the jump from start to landing.
        /// </summary>
        [SerializeField] float jumpTime;
        [SerializeField] float internalGravity;

        [Header("Control Information")]
        [SerializeField] bool _isAnyMovePressed;
        [SerializeField] bool _isMovingBackwards;
        [SerializeField] bool _isMovingSideways;
        [SerializeField] bool _isWalking;
        [SerializeField] bool _isRunning;
        [SerializeField] bool _isCrouching;
        [SerializeField] bool _hasJumped;
        [SerializeField] bool _inAir;
        [SerializeField] bool _canCoyote;
        [SerializeField] float _fallSpeedAcceleration = 2f;
        [SerializeField] float _targetMoveSpeed;
        /// <summary>
        /// The velocity to be applied for the current frame.
        /// </summary>
        [SerializeField] Vector3 _frameVelocity;

        /// <summary>
        /// The input values performed in the current frame.
        /// </summary>
        FrameInput _frameInput;
        /// <summary>
        /// The input values performed in the current frame.
        /// </summary>
        public FrameInput FrameInput => _frameInput;
        MoveStates _targetMoveState;


        #region Public properties

        public bool IsWalking => _isWalking;
        /// <summary>
        /// If holding [Run] key and speed is greater than run threshold.
        /// </summary>
        public bool IsRunning => _isRunning;
        public bool IsCrouching => _isCrouching;
        public bool IsMoving
        {
            get => rb.velocity.magnitude > 0.01f;
        }
        public bool IsAnyMoveKeyPressed
        {
            get => _frameInput.Move != Vector2.zero;
        }
        public bool IsGrounded
        {
            get => groundChecker.IsGrounded;
        }
        public bool IsFalling
        {
            get => _frameVelocity.y < 0f;
        }
        public bool CanRun
        {
            get => !_isMovingBackwards && !_isMovingSideways && IsGrounded;
        }
        /// <summary>
        /// Whether if the Player can Camera bob in FPP.
        /// </summary>
        public bool CanBob
        {
            get => IsGrounded && IsMoving;
        }
        public bool CanCoyoteJump
        {
            get => groundChecker.CanCoyoteJump;
        }
        /// <summary>
        /// The world space velocity of the player's rigidbody.
        /// </summary>
        public Vector3 Velocity
        {
            get => rb.velocity;
        }
        /// <summary>
        /// The local velocity of the player's rigidbody.
        /// </summary>
        public Vector3 LocalVelocity
        {
            get => Player.MainCamera.transform.InverseTransformDirection(Velocity);
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
        /// <summary>
        /// Horizontal forward direction of the player's camera, with the y value zeroed out.
        /// </summary>
        Vector3 CameraForward
        {
            get
            {
                Vector3 camForward = Player.Forward;
                camForward.y = 0f;
                return camForward;
            }
        }

        #endregion


        #region Controls events

        /// TODO: state machine weirdness
        /// <summary>
        /// Called whenever the player enters/exits crouching stance.
        /// bool is true if ENTERED crouch. false if EXITED.
        /// </summary>
        public event Action<bool> OnCrouch;
        public event Action<float> OnTurn;

        #endregion


        [Header("Components")]
        /// <summary>
        /// The Player's 3D model.
        /// </summary>
        public Transform Model;
        [SerializeField] Rigidbody rb;
        public Rigidbody Rigidbody => rb;
        public GroundChecker groundChecker;
        
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

        /// <summary>
        /// Cached values of the Player's attributes.
        /// Key is Attribute Id, Value is the Value of that Attribute.
        /// </summary>
        Dictionary<string, float> _cachedAttributeValues = new();

        void RetrieveAttributes()
        {
            /// Attributes to cache
            string[] attrIds = new[]
            {
                "walk_speed",
                "move_speed",
                "run_speed",
                "crouch_speed",
                "jump_velocity",
            };

            foreach (string id in attrIds)
            {
                var attr = Player.Attributes.Get(id);
                _cachedAttributeValues[id] = attr.Value;
                attr.OnValueChanged += UpdateAttributeValue;
            }

            void UpdateAttributeValue(object sender, Attributes.AttributeValueChangedContext ctx)
            {
                var attr = (Attributes.Attribute) sender;
                _cachedAttributeValues[attr.Id] = attr.Value;
            }
            
            /// Initialize move speed
            _targetMoveSpeed = _cachedAttributeValues["move_speed"];
        }

        #endregion


        void FixedUpdate()
        {
            HandleDirection();
            HandleTurning();
            HandleRotation();
            // HandleManualGravity();
            HandleJump();
            ApplyMovement();
            UpdateStates();
        }

        void UpdateStates()
        {
            if (IsMoving && IsAnyMoveKeyPressed && !_hasJumped)
            {
                Player.MoveStateMachine.ToState(_targetMoveState);
            }
            else if (_isCrouching)
            {
                Player.MoveStateMachine.ToState(MoveStates.Crouch);
            }
            else
            {
                Player.MoveStateMachine.ToState(MoveStates.Idle);
            }
        }


        #region Player input callbacks

        void OnInputMove(InputAction.CallbackContext input)
        {
            if (!AllowMovement) return;

            _frameInput.Move = input.ReadValue<Vector2>();
            _isAnyMovePressed = _frameInput.Move.x != 0f || _frameInput.Move.y != 0f;
            _isMovingSideways = _frameInput.Move.x != 0f;
            _isMovingBackwards = _frameInput.Move.y < 0f;

            CancelRunIfNotRunningForwards();
        }

        void OnInputRun(InputAction.CallbackContext input)
        {
            if (!AllowMovement) return;

            if (input.started && CanRun && !Player.FPP.IsPerforming)
            {
                ToggleCrouch(false); /// in the future, the player should be able to crouch-run
                ToggleWalk(false);

                ToggleRun(true);
            }
            else if (input.canceled)
            {
                ToggleRun(false);
            }
        }

        void OnInputWalkToggle(InputAction.CallbackContext context)
        {
            ToggleRun(false);
            ToggleWalk(!IsWalking);
        }

        void OnInputJump(InputAction.CallbackContext input)
        {
            if (!AllowMovement) return;

            if (input.started)
            {
                ToggleCrouch(false); /// forces uncrouch when jumping (salt)

                if (!IsGrounded || !CanCoyoteJump) return;

                if (Player.HasStaminaForJump)
                {
                    _hasJumped = true;
                }
                else
                {
                    /// do a "half" jump
                }
            }
        }

        void OnInputCrouch(InputAction.CallbackContext input)
        {
            if (!AllowMovement) return;

            if (CrouchIsToggle)
            {
                if (input.started)
                {
                    ToggleRun(false);

                    ToggleCrouch(!IsCrouching);
                }
            }
            else // is hold
            {
                ToggleRun(false);
                
                if (input.started)
                {
                    ToggleCrouch(true);
                }
                else if (input.canceled)
                {
                    ToggleCrouch(false);
                }
            }
        }

        #endregion


        void CancelRunIfNotRunningForwards()
        {
            if (_isMovingSideways || _isMovingBackwards)
            {
                ToggleRun(false);
            }
        }

        void ToggleRun(bool run)
        {
            if (run == _isRunning) return; /// if the value set is same as the current state

            if (run)
            {
                _targetMoveSpeed = _cachedAttributeValues["run_speed"];
                _targetMoveState = MoveStates.Run;
            }
            else
            {
                _targetMoveSpeed = _cachedAttributeValues["move_speed"];
                _targetMoveState = default;
            }

            _isRunning = run;
        }
        

        void ToggleWalk(bool walk)
        {
            if (walk == _isWalking) return; /// if the value set is same as the current state
            
            if (walk)
            {
                _targetMoveSpeed = _cachedAttributeValues["walk_speed"];
                _targetMoveState = MoveStates.Walk;
            }
            else
            {
                _targetMoveSpeed = _cachedAttributeValues["move_speed"];
                _targetMoveState = default;
            }

            _isWalking = walk;
        }

        void HandleJump()
        {
            if (!_hasJumped) return;
            if (!IsGrounded || !CanCoyoteJump) /// it's here for reduced checks per fixedUpdate
            {
                _hasJumped = false;
                return;
            }

            _hasJumped = false;
            float apexTime = jumpTime / 2f; /// time required to reach the highest point of the jump
            float initialJumpVelocity = (2f * jumpHeight) / apexTime; /// the initial speed of the upward motion of the player's jump

            Vector3 jumpingVelocity = rb.velocity;
            jumpingVelocity.y = initialJumpVelocity;
            rb.velocity = jumpingVelocity;

            Player.MoveStateMachine.ToState(MoveStates.Jump);
        }

        void HandleDirection()
        {
            var x = _frameInput.Move.x * Player.Right;
            var y = _frameInput.Move.y * CameraForward.normalized;
            _frameVelocity = x + y;
            _frameVelocity.Normalize();
        }

        Quaternion _desiredRotation;

        void HandleTurning()
        {
            if (!IsMoving) return;
            {
                Vector3 modelForward = Model.forward;
                Vector3 cameraForward = Player.Forward;

                if (Vector3.Angle(modelForward, cameraForward) > TurningAngle)
                {
                    Vector3 cross = Vector3.Cross(modelForward, cameraForward);

                    if (cross.y > 0) /// turn right
                    {
                        OnTurn?.Invoke(1f);
                    }
                    else if (cross.y < 0) /// turn left
                    {
                        OnTurn?.Invoke(-1f);
                    }

                    Player.MoveStateMachine.ToState(MoveStates.Turn);
                }
            }
        }

        void HandleRotation()
        {
            /// Handle the rotation of the Player model only when moving
            if (IsMoving)
            {
                Quaternion targetRotation = Quaternion.LookRotation(CameraForward.normalized);
                Model.rotation = Quaternion.Slerp(Model.rotation, targetRotation, RotationDamping * Time.fixedDeltaTime);
            }
        }

        // void HandleManualGravity()
        // {
        //     if (!UseManualGravity) return;
            
        //     var targetVelocity = rb.velocity;

        //     if (IsGrounded) /// grounding force only
        //     {
        //         targetVelocity.y = -0.1f;

        //     }
        //     else if (IsFalling) /// increasing fall speed
        //     {
        //         targetVelocity.y += internalGravity * _fallSpeedAcceleration * Time.fixedDeltaTime; /// gravity squared
        //     }
        //     else /// normal gravity
        //     {
        //         targetVelocity.y += internalGravity * Time.fixedDeltaTime; /// gravity squared
        //     }

        //     rb.velocity = targetVelocity;
        // }

        void ApplyMovement()
        {
            var targetVelocity = _frameVelocity * (_targetMoveSpeed * Time.fixedDeltaTime);
            targetVelocity.y = rb.velocity.y;
            /// No acceleration
            // rb.velocity = targetVelocity * TimeScale;
            /// With acceleration
            rb.velocity = Vector3.Lerp(rb.velocity, targetVelocity, MoveAcceleration * Time.fixedDeltaTime);
        }

        void ToggleCrouch(bool crouch)
        {
            if (crouch == _isCrouching) return; /// if the value set is same as the current state
            
            if (crouch)
            {
                _targetMoveSpeed = _cachedAttributeValues["crouch_speed"];
                OnCrouch?.Invoke(true);
            }
            else
            {
                _targetMoveSpeed = _cachedAttributeValues["move_speed"];
                OnCrouch?.Invoke(false);
            }
            _isCrouching = crouch;
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
            if (!inputs.TryGetValue(name, out var input)) return;
            
            if (enabled)
            {
                input.Enable();
                Game.Console.LogDebug($"Enabled control '{name}' for '{name} [{Player.InstanceId}]'");
            }
            else
            {
                input.Disable();
                Game.Console.LogDebug($"Disabled control '{name}' for '{name} [{Player.InstanceId}]'");
            }
        }

        public void SetControls(string[] controls, bool enabled)
        {
            if (controls.Length == 0) return;

            foreach (string id in controls)
            {
                SetControl(id, enabled);
            }
        }

        #endregion
    }
}