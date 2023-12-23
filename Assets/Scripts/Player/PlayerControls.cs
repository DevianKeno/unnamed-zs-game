using System;
using Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using URMG.Systems;
using URMG.Items;
using URMG.Interactions;

namespace URMG.Player
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
        PlayerCore player;
        public PlayerCore Player { get => player; }

        [SerializeField] Camera cam;
        public Transform CharacterBody;
        public CharacterController controller;
        public Transform GroundCheck;
        public LayerMask GroundMask;
        public Camera Cam { get => cam; }
        bool _allowCamMovement = true;

        public const float MoveSpeed = 6f;
        public const float WalkSpeed = MoveSpeed * 0.5f;
        public const float JumpForce = 1.5f;
        public const float Gravity = -9.81f;
        public const float GroundDistance = 0.4f;

        RaycastHit hit;
        Ray ray;
        public float sphereRadius = 0;
        public float interactMaxDistance = 1;
        /// <summary>
        /// The interactable object the Player is currently looking at.
        /// </summary>
        IInteractable lookingAt;
        /// <summary>
        /// The movement values to be added for the current frame.
        /// </summary>
        Vector3 _movement;
        Vector3 FallSpeed;
        Vector3 CrouchPosition;
        bool _isMoving;
        public bool IsMoving { get => _isMoving; }
        bool _isGrounded;
        public bool IsGrounded { get => _isGrounded; }
        bool _isRunning;
        public bool isRunning { get => _isRunning; }
        bool _isCrouching;
        public bool isCrouching { get => _isCrouching; }

        public event EventHandler OnMoveStart;
        public event EventHandler OnMoveStop;

        PlayerActions playerActions;
        FrameInput frameInput;
        PlayerInput playerInput;
        InputAction moveInput;
        InputAction jumpInput;
        InputAction runInput;
        InputAction crouchInput;
        InputAction primaryInput;
        InputAction secondaryInput;
        InputAction interactInput;
        InputAction inventoryInput;
        InputAction hotbarInput;
        [SerializeField] Rigidbody rb;
        [SerializeField] CinemachineVirtualCamera vCam;
        CinemachinePOV vCamPOV;

        void Awake()
        {
            vCamPOV = vCam.GetCinemachineComponent<CinemachinePOV>();
            player = GetComponent<PlayerCore>();
            playerActions = GetComponent<PlayerActions>();
            controller = GetComponent<CharacterController>();
            InitControls();
        }

        void InitControls()
        {
            playerInput = GetComponent<PlayerInput>();
            moveInput = playerInput.actions.FindAction("Move");
            jumpInput = playerInput.actions.FindAction("Jump");
            runInput = playerInput.actions.FindAction("Run");
            crouchInput = playerInput.actions.FindAction("Crouch");
            primaryInput = playerInput.actions.FindAction("Primary");
            secondaryInput = playerInput.actions.FindAction("Secondary");
            interactInput = playerInput.actions.FindAction("Interact");
            inventoryInput = playerInput.actions.FindAction("Inventory");
            hotbarInput = playerInput.actions.FindAction("Hotbar");
        }

        void Start()
        {
            Game.Tick.OnTick += Tick;

            moveInput.Enable();
            jumpInput.Enable();
            runInput.Enable();
            crouchInput.Enable();
            primaryInput.Enable();
            secondaryInput.Enable();
            interactInput.Enable();
            inventoryInput.Enable();
            hotbarInput.Enable();

            jumpInput.performed += OnJumpX;              // Pressed Space (default)
            runInput.started += OnRunX;                 // Pressed Shift (default)  
            runInput.canceled += OnRunX;                // Released Shift
            crouchInput.performed += OnCrouchX;         // Pressed LCtrl (default)
            primaryInput.performed += OnPrimaryX;        // Pressed LMB (default)
            secondaryInput.performed += OnSecondaryX;    // Pressed RMB (default)
            interactInput.performed += OnInteractX;      // Pressed F (default)
            inventoryInput.performed += OnInventoryX;    // Pressed Tab/E (default)
            hotbarInput.performed += OnHotbarSelect;    // Pressed Tab/E (default)
        }

        void OnHotbarSelect(InputAction.CallbackContext context)
        {            
            playerActions.SelectHotbar(int.Parse(context.control.displayName));
        }

        void OnPrimaryX(InputAction.CallbackContext context)
        {
            playerActions.PerformPrimary();
        }

        void OnSecondaryX(InputAction.CallbackContext context)
        {
            playerActions.PerformSecondary();
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
        
        void OnInteractX(InputAction.CallbackContext context)
        {
            if (lookingAt == null) return;
            
            playerActions.Interact(lookingAt);
        }
        void OnInventoryX(InputAction.CallbackContext context)
        {
            playerActions.ToggleInventory();
            AllowCameraMovement(!Game.UI.InventoryUI.IsVisible);
        }

        void OnDisable()
        {
            Game.Tick.OnTick -= Tick;
            moveInput.Disable();
            jumpInput.Disable();
            runInput.Disable();
            interactInput.Disable();
            inventoryInput.Disable();
        }

        void Tick(object sender, TickEventArgs e)
        {
            CheckMoving();
            HandleMovement();
            CheckLookingAt();
        }

        void CheckLookingAt()
        {
            // Cast a ray from the center of the screen
            ray = cam.ScreenPointToRay(new Vector2(Screen.width / 2, Screen.height / 2));

            if (Physics.SphereCast(ray, sphereRadius, out RaycastHit hit, interactMaxDistance, LayerMask.GetMask("Interactable")))
            {
                lookingAt = hit.collider.gameObject.GetComponent<IInteractable>();

                if (hit.collider.CompareTag("Item"))
                {
                    Game.UI.InteractIndicator.Show(new()
                    {
                        Action = lookingAt.Action,
                        Object = lookingAt.Name,
                    });
                }
            } else
            {
                lookingAt = null;
                Game.UI.InteractIndicator.Hide();
            }
        }

        // void OnDrawGizmos()
        // {
        //     Gizmos.DrawLine(ray.origin, ray.origin + ray.direction * (interactMaxDistance + sphereRadius));
        //     Gizmos.DrawWireSphere(ray.origin + ray.direction * (interactMaxDistance + sphereRadius), sphereRadius);
        //     Gizmos.DrawWireSphere(ray.origin + ray.direction * (interactMaxDistance + sphereRadius), sphereRadius);
        // }

        bool CheckGrounded()
        {
            return _isGrounded = Physics.CheckSphere(GroundCheck.position, GroundDistance, GroundMask) && FallSpeed.y < 0;
        }
        void CheckMoving()
        {
            if (rb.velocity == Vector3.zero) _isMoving = false;
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
            // Moves player relative to the camera
            // _movement = frameInput.move.x * cam.transform.right + frameInput.move.y * cam.transform.forward;
            // _movement.y = 0f; 
            // _movement.Normalize();
            // _movement *= MoveSpeed * Time.deltaTime;

            Vector3 cameraForward = cam.transform.forward;
            cameraForward.y = 0f;
            _movement = frameInput.move.x * cam.transform.right + frameInput.move.y * cameraForward.normalized;
            _movement.Normalize();
        }

        void ApplyMovement()
        {
            Quaternion dRotation = Quaternion.Euler(cam.transform.eulerAngles.x, 0f, 0f);
            CharacterBody.rotation = Quaternion.Slerp(CharacterBody.rotation, dRotation, Time.deltaTime * CamSensitivity);

            float MovementSpeed = WalkSpeed;

            if (_isCrouching) MovementSpeed *= 0.3f;
            else MovementSpeed = WalkSpeed;

            if (_isRunning) controller.Move(_movement * MoveSpeed * Time.deltaTime);
            else controller.Move(_movement * MovementSpeed * Time.deltaTime);
            
        }
        void Crouch()
        {
            _isCrouching = !_isCrouching;
            if (_isCrouching) CrouchPosition = new Vector3(vCam.transform.position.x, vCam.transform.position.y * 0.5f, vCam.transform.position.z);
            else CrouchPosition = new Vector3(vCam.transform.position.x, vCam.transform.position.y * 2f, vCam.transform.position.z);
            
            // vCam.transform.position = Vector3.Lerp(vCam.transform.position, CrouchPosition, 1);
            LeanTween.value(gameObject, vCam.transform.position.y, CrouchPosition.y, 0.3f)
            .setOnUpdate(
                (i) =>
                {
                    vCam.transform.position = new Vector3
                    (vCam.transform.position.x,
                    i,
                    vCam.transform.position.z);
                }
            ).setEaseOutExpo();
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
        
        public void AllowCameraMovement(bool value)
        {
            if (value)
            {
                _allowCamMovement = true;
                vCamPOV.m_VerticalAxis.m_MaxSpeed = CamSensitivity;
                vCamPOV.m_HorizontalAxis.m_MaxSpeed = CamSensitivity;
            } else
            {
                _allowCamMovement = false;
                vCamPOV.m_VerticalAxis.m_MaxSpeed = 0f;
                vCamPOV.m_HorizontalAxis.m_MaxSpeed = 0f;
            }
        }
    }
}