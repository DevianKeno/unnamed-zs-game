using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Cinemachine;
using UZSG.Systems;

namespace UZSG.Player
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
        public const float CamSensitivity = 0.32f;
        float xRotation = 0f;
        PlayerEntity _player;
        public PlayerEntity Player { get => _player; }

        /// <summary>
        /// Cached movement speed from the Player. This should subscribe to the OnValueChanged event.
        /// </summary>
        float _moveSpeed;
        public float CrouchSpeedMultiplier = 0.7f;
        [SerializeField] Camera _cam;
        public Transform CharacterBody;
        public CharacterController _controller;
        public Transform GroundCheck;
        public LayerMask GroundMask;
        Vector3 prevPos;
        public Camera Cam { get => _cam; }

        public const float MoveSpeed = 10f;
        public const float WalkSpeed = MoveSpeed * 0.5f;
        public const float JumpForce = 1.5f;
        public const float Gravity = -9.81f;
        public const float GroundDistance = 0.4f;
        /// <summary>
        /// The movement values to be added for the current frame.
        /// </summary>
        Vector3 _frameMovement;
        Vector3 FallSpeed;
        float CrouchPosition;
        bool _isTransitioning;
        bool _isMoving;
        public bool IsMoving { get => _isMoving; }
        bool _isGrounded;
        public bool IsGrounded { get => _isGrounded; }
        bool _isRunning;
        public bool isRunning { get => _isRunning; }
        bool _isCrouching;
        public bool isCrouching { get => _isCrouching; }

        float _magnitude;
        public float ControllerMagnitude { get => _magnitude; }
        public event EventHandler OnMoveStart;
        public event EventHandler OnMoveStop;

        PlayerActions _actions;
        FrameInput frameInput;
        PlayerInput _input;
        InputAction moveInput;
        InputAction jumpInput;
        InputAction runInput;
        InputAction crouchInput;
        [SerializeField] CinemachineVirtualCamera vCam;
        CinemachinePOV _vCamPOV;

        void Awake()
        {
            _player = GetComponent<PlayerEntity>();
            _actions = GetComponent<PlayerActions>();
            _input = GetComponent<PlayerInput>();
            _controller = GetComponent<CharacterController>();
            _vCamPOV = vCam.GetCinemachineComponent<CinemachinePOV>();
        }

        internal void Initialize()
        {
            InitializeControls();
        }

        void InitializeControls()
        {
            moveInput = _input.actions.FindAction("Move");
            jumpInput = _input.actions.FindAction("Jump");
            runInput = _input.actions.FindAction("Run");
            crouchInput = _input.actions.FindAction("Crouch");
            

            moveInput.Enable();
            jumpInput.Enable();
            runInput.Enable();
            crouchInput.Enable();

            /*  performed = Pressed or released
                started = Pressed
                canceled = Released
            */
            jumpInput.performed += OnJumpX;                 // Space (default)
            runInput.started += OnRunX;                     // Shift (default)  
            runInput.canceled += OnRunX;                    // Shift
            crouchInput.performed += OnCrouchX;             // LCtrl (default)

            Game.Tick.OnTick += Tick;
            _player.OnDoneInit += PlayerDoneInit;
        }

        void PlayerDoneInit(object sender, EventArgs e)
        {
            _moveSpeed = _player.Attributes.GetAttributeFromId("move_speed").Value;
        }

        void OnDisable()
        {
            Game.Tick.OnTick -= Tick;
            moveInput.Disable();
            jumpInput.Disable();
            runInput.Disable();
        }

        void OnMoveX(InputAction.CallbackContext context)
        {
            Debug.Log("I tried moving...");
            var move = moveInput.ReadValue<Vector2>();

            Vector3 camForward = _cam.transform.forward;
            camForward.y = 0f; // Inhibit vertical movement

            // Move player relative to camera direction
            _frameMovement = (move.x * _cam.transform.right) + (move.y * camForward.normalized);
            _frameMovement.Normalize();
        }

        void OnJumpX(InputAction.CallbackContext context)
        {
            if(CheckGrounded())
            {
                if (_player.Attributes.GetAttributeFromId("stamina").Value < 10) return;

                _player.sm.ToState(_player.sm.States[PlayerStates.Jump]);
                FallSpeed.y = Mathf.Sqrt(JumpForce * -2f * Gravity);
            }
        }

        void OnRunX(InputAction.CallbackContext context)
        {
            if (_isCrouching)
            {
                Crouch();
            }

            if (_isRunning)
            {
                _isRunning = false;
                _player.sm.ToState(_player.sm.States[PlayerStates.Run]);
            } else
            {
                _isRunning = true;
                _player.sm.ToState(_player.sm.States[PlayerStates.Idle]);
            }
        }

        void OnCrouchX(InputAction.CallbackContext context)
        {
            Crouch();
        }

        void Tick(object sender, TickEventArgs e)
        {
            CheckMoving();
            HandleMovement();
        }

        bool CheckGrounded()
        {
            return _isGrounded = Physics.CheckSphere(GroundCheck.position, GroundDistance, GroundMask) && FallSpeed.y < 0;
        }

        void CheckMoving()
        {
            if (_magnitude == 0f) _isMoving = false;
            else _isMoving = true;
        }

        FrameInput GatherInput()
        {
            return new()
            {
                move = moveInput.ReadValue<Vector2>()
            };
        }

        void Update()
        {
            frameInput = GatherInput();
        }

        void HandleMovement()
        {
            HandleDirection();
            ApplyMovement();
            ApplyGravity();
        }

        void HandleDirection()
        {
            Vector3 camForward = _cam.transform.forward;
            camForward.y = 0f;

            // Move player relative to the camera
            _frameMovement = (frameInput.move.x * _cam.transform.right) + (frameInput.move.y * camForward.normalized);
            _frameMovement.Normalize();
        }

        void ApplyMovement()
        {
            Quaternion dRotation = Quaternion.Euler(_cam.transform.eulerAngles.x, 0f, 0f);
            _cam.transform.rotation = Quaternion.Slerp(CharacterBody.rotation, dRotation, Time.fixedDeltaTime * CamSensitivity);
        
            _controller.Move(_frameMovement * (_moveSpeed * Time.fixedDeltaTime));

            _magnitude = new Vector3(_controller.velocity.x, 0, _controller.velocity.z).magnitude;    
        }

        void Crouch()
        {
            if (_isTransitioning) return;

            _player.sm.ToState(_player.sm.States[PlayerStates.Crouch]);

            _isTransitioning = !_isTransitioning;
            _isCrouching = !_isCrouching;

            float TransitionSpeed;
            
            if (_isCrouching)
            { 
                _moveSpeed *= CrouchSpeedMultiplier;
                CrouchPosition = vCam.transform.position.y - 1f;
                TransitionSpeed = 0.3f;
            } else
            {
                _moveSpeed /= CrouchSpeedMultiplier;
                CrouchPosition = vCam.transform.position.y + 1f;
                TransitionSpeed = 0.3f;
            }

            LeanTween.value(gameObject, vCam.transform.position.y, CrouchPosition, TransitionSpeed)
            .setOnUpdate( (i) =>
                {   
                    vCam.transform.position = new Vector3(
                        vCam.transform.position.x,
                        i,
                        vCam.transform.position.z
                    );
                }
            ).setOnComplete( () =>
                {
                    _isTransitioning = false;
                }
            ).setEaseOutExpo();
        }

        void ApplyGravity()
        {
            if (CheckGrounded())
            {
                FallSpeed.y = -2f;
            }

            FallSpeed.y += Gravity * Time.fixedDeltaTime;
            _controller.Move(FallSpeed * Time.fixedDeltaTime);
        }
        
        public void AllowControls(bool value)
        {
            if (value)
                moveInput.Enable();
            else
                moveInput.Disable();
        }
    }
}