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
        public Player Player;
        [Space]

        [Header("Parameters")]
        public bool AllowMovement;
        public float MoveAcceleration = 10f;
        public float RotationDamping = 6f;
        public float TurningAngle = 120f;
        public bool RunIsToggle = false;
        public bool CrouchIsToggle = true;

        [Header("Serialized Fields")]
        [SerializeField] bool _isAnyMovePressed;
        [SerializeField] bool _isMovingBackwards,
            _isMovingSideways,
            _isWalking,
            _isRunning,
            _hasJumped;
        float _targetMoveSpeed;

        bool _isTransitioningCrouch;
        float _crouchingCameraPosition;
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
            /// For some reason, this does not equate to zero,
            /// even when the player is not moving and standing still
            /// Bug?
            // get => rb.velocity != Vector3.zero;
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
        public bool CanRun
        {
            get => !_isMovingBackwards && !_isMovingSideways;
        }
        /// <summary>
        /// Whether if the Player can Camera bob in FPP.
        /// </summary>
        public bool CanBob
        {
            get => IsGrounded;
        }
        public bool CanCoyoteJump
        {
            get => groundChecker.CanCoyoteJump;
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
        public Transform Model;
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

        /// <summary>
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
                "jump_height",
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
            
            _targetMoveSpeed = _cachedAttributeValues["move_speed"];
        }

        #endregion


        void FixedUpdate()
        {
            HandleDirection();
            HandleTurning();
            HandleRotation();
            ApplyMovement();
            UpdateStates();
        }

        void UpdateStates()
        {
            if (_isAnyMovePressed && IsMoving && !_hasJumped)
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
            _isAnyMovePressed = _frameInput.Move.x != 0f || _frameInput.Move.y != 0f;
            _isMovingSideways = _frameInput.Move.x != 0f;
            _isMovingBackwards = _frameInput.Move.y < 0f;

            CancelRunIfNotRunningForwards();
        }

        void OnInputRun(InputAction.CallbackContext context)
        {
            if (!AllowMovement) return;
            if (!IsGrounded) return;
            
            CancelRunIfNotRunningForwards();

            if (context.started || CanRun)
            {
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

            if (CrouchIsToggle)
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


        void CancelRunIfNotRunningForwards()
        {
            if (_isMovingSideways || _isMovingBackwards)
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
            if (!IsGrounded) return;

            _hasJumped = true;
            Vector3 jumpVelocity = rb.velocity;
            jumpVelocity.y = _cachedAttributeValues["jump_height"];
            rb.velocity = jumpVelocity;
            Player.MoveStateMachine.ToState(MoveStates.Jump);
        }

        void HandleDirection()
        {
            var x = _frameInput.Move.x * Player.MainCamera.transform.right;
            var y = _frameInput.Move.y * CameraForward.normalized;
            _frameVelocity = x + y;
            _frameVelocity.Normalize();
        }

        void HandleRotation()
        {
            /// Handle the rotation of the Player model only when moving
            if (IsMoving)
            {
                Quaternion targetRotation = Quaternion.LookRotation(CameraForward.normalized);
                Model.rotation = Quaternion.Slerp(Model.rotation, targetRotation, RotationDamping * Time.deltaTime);
            }
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
                        Player.Animator.SetFloat("turn", 1f);
                    }
                    else if (cross.y < 0) /// turn left
                    {
                        Player.Animator.SetFloat("turn", -1f);
                    }

                    Player.MoveStateMachine.ToState(MoveStates.Turn);
                }
            }
        }

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
            if (_isTransitioningCrouch) return;
            _isTransitioningCrouch = true;

            float transitionSpeed = 0.66f;
            Player.MoveStateMachine.ToState(MoveStates.Crouch);
            
            if (crouch)
            {
                _targetMoveSpeed = _cachedAttributeValues["crouch_speed"];
                _crouchingCameraPosition = Player.FPP.Camera.transform.position.y - 1f;
                Player.InfoHUD.FadeVignette(1f);
            }
            else
            {
                _targetMoveSpeed = _cachedAttributeValues["move_speed"];
                _crouchingCameraPosition = Player.FPP.Camera.transform.position.y + 1f;
                Player.InfoHUD.FadeVignette(0f);
            }
            _isCrouching = crouch;

            LeanTween.value(gameObject, Player.FPP.Camera.transform.position.y, _crouchingCameraPosition, transitionSpeed)
            .setOnUpdate((float i) =>
            {   
                Player.FPP.Camera.transform.position = new Vector3(
                    Player.FPP.Camera.transform.position.x,
                    i,
                    Player.FPP.Camera.transform.position.z);
            })
            .setOnComplete(() =>
            {
                _isTransitioningCrouch = false;
            })
            .setEaseOutExpo();
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