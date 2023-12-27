using System;
using Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
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

    [RequireComponent(typeof(PlayerCore))]
    public class PlayerControls : MonoBehaviour
    {
        public const float CamSensitivity = 0.32f;
        float xRotation = 0f;
        PlayerCore _player;
        public PlayerCore Player { get => _player; }

        [SerializeField] Camera cam;
        public Transform CharacterBody;
        public CharacterController controller;
        public Transform GroundCheck;
        public LayerMask GroundMask;
        public Camera Cam { get => cam; }

        public const float MoveSpeed = 6f;
        public const float WalkSpeed = MoveSpeed * 0.5f;
        public const float JumpForce = 1.5f;
        public const float Gravity = -9.81f;
        public const float GroundDistance = 0.4f;
        /// <summary>
        /// The movement values to be added for the current frame.
        /// </summary>
        Vector3 _movement;
        Vector3 FallSpeed;
        float CrouchPosition;
        bool isTransitioning;
        bool _isMoving;
        public bool IsMoving { get => _isMoving; }
        bool _isGrounded;
        public bool IsGrounded { get => _isGrounded; }
        bool _isRunning;
        public bool isRunning { get => _isRunning; }
        bool _isCrouching;
        public bool isCrouching { get => _isCrouching; }

        float _Magnitude;
        public float ControllerMagnitude { get => _Magnitude; }
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
            _player = GetComponent<PlayerCore>();
            _actions = GetComponent<PlayerActions>();
            _input = GetComponent<PlayerInput>();
            controller = GetComponent<CharacterController>();
            _vCamPOV = vCam.GetCinemachineComponent<CinemachinePOV>();
            InitControls();
        }

        void InitControls()
        {
            moveInput = _input.actions.FindAction("Move");
            jumpInput = _input.actions.FindAction("Jump");
            runInput = _input.actions.FindAction("Run");
            crouchInput = _input.actions.FindAction("Crouch");
        }

        void Start()
        {
            Game.Tick.OnTick += Tick;

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
        }

        void OnDisable()
        {
            Game.Tick.OnTick -= Tick;
            moveInput.Disable();
            jumpInput.Disable();
            runInput.Disable();
        }

        void OnJumpX(InputAction.CallbackContext context)
        {
            if(CheckGrounded())
            {
                FallSpeed.y = Mathf.Sqrt(JumpForce * -2f * Gravity);
            }
        }

        void OnRunX(InputAction.CallbackContext context)
        {
            _isRunning = !_isRunning;            
            if (_isCrouching)
            {
                Crouch();
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
            if (_Magnitude == 0f) _isMoving = false;
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
            Vector3 cameraForward = cam.transform.forward;
            cameraForward.y = 0f;
            // Moves player relative to the camera
            _movement = frameInput.move.x * cam.transform.right + frameInput.move.y * cameraForward.normalized;
            _movement.Normalize();
        }

        void ApplyMovement()
        {
            Quaternion dRotation = Quaternion.Euler(cam.transform.eulerAngles.x, 0f, 0f);
            Cam.transform.rotation = Quaternion.Slerp(CharacterBody.rotation, dRotation, Time.deltaTime * CamSensitivity);

            float MovementSpeed = WalkSpeed;

            if (_isCrouching) MovementSpeed *= 0.3f;
            else MovementSpeed = WalkSpeed;

            if (_isRunning) controller.Move(MoveSpeed * Time.deltaTime * _movement);
            else controller.Move(MovementSpeed * Time.deltaTime * _movement);

            _Magnitude = new Vector3(controller.velocity.x, 0, controller.velocity.z).magnitude;
            
        }
        void Crouch()
        {
            if (isTransitioning) return;

            _player.sm.ToState(_player.sm.States[PlayerStates.Crouch]);

            isTransitioning = !isTransitioning;
            _isCrouching = !_isCrouching;

            float TransitionSpeed;
            
            if (_isCrouching)
            { 
                CrouchPosition = vCam.transform.position.y - 1f;
                TransitionSpeed = 0.3f;
            }
            else
            {
                CrouchPosition = vCam.transform.position.y + 1f;
                TransitionSpeed = 0.3f;
            }
            LeanTween.value(gameObject, vCam.transform.position.y, CrouchPosition, TransitionSpeed)
            .setOnUpdate( (i) =>
                {   
                    vCam.transform.position = new Vector3
                    (vCam.transform.position.x,
                    i,
                    vCam.transform.position.z);
                }
            ).setOnComplete( () => {isTransitioning = false;} )
            .setEaseOutExpo();
        }

        void ApplyGravity()
        {
            if (CheckGrounded())
            {
                FallSpeed.y = -2f;
            }
            FallSpeed.y += Gravity * Time.deltaTime;
            controller.Move(FallSpeed * Time.deltaTime);
        }
        
        public void AllowControls(bool value)
        {
            if (value) moveInput.Enable();
            else moveInput.Disable();
        }
    }
}