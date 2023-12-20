using System;
using Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using URMG.Core;
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
    
    PlayerCore player;
    public PlayerCore Player { get => player; }

    [SerializeField] Camera cam;
    public Camera Cam { get => cam; }
    bool _allowCamMovement = true;

    public const float MoveSpeed = 3f;
    public const float WalkSpeed = MoveSpeed * 0.7f;
    public const float JumpForce = 1f;
    
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
    bool _isMoving;
    public bool IsMoving { get => _isMoving; }
    bool _isGrounded;
    public bool IsGrounded { get => _isGrounded; }

    public event EventHandler OnMoveStart;
    public event EventHandler OnMoveStop;

    PlayerActions playerActions;
    FrameInput frameInput;
    PlayerInput playerInput;
    InputAction moveInput;
    InputAction jumpInput;
    InputAction interactInput;
    InputAction inventoryInput;
    [SerializeField] Rigidbody rb;
    [SerializeField] CinemachineVirtualCamera vCam;
    CinemachinePOV vCamPOV;

    void Awake()
    {
        vCamPOV = vCam.GetCinemachineComponent<CinemachinePOV>();
        player = GetComponent<PlayerCore>();
        playerActions = GetComponent<PlayerActions>();
        InitControls();
    }

    void InitControls()
    {
        playerInput = GetComponent<PlayerInput>();
        moveInput = playerInput.actions.FindAction("Move");
        jumpInput = playerInput.actions.FindAction("Jump");
        interactInput = playerInput.actions.FindAction("Interact");
        inventoryInput = playerInput.actions.FindAction("Inventory");
    }

    void Start()
    {
        Game.Tick.OnTick += Tick;

        moveInput.Enable();
        jumpInput.Enable();
        interactInput.Enable();
        inventoryInput.Enable();

        jumpInput.performed += OnJumpX;              // Pressed Space (default)
        interactInput.performed += OnInteractX;      // Pressed F (default)
        inventoryInput.performed += OnInventoryX;    // Pressed Tab/E (default)
    }

    void OnJumpX(InputAction.CallbackContext context)
    {
        // Make the player Jump
        // if (_isGrounded)
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
    }

    void HandleDirection()
    {
        // Moves player relative to the camera
        _movement = frameInput.move.x * cam.transform.right + frameInput.move.y * cam.transform.forward;
        _movement.y = 0f; 
        _movement.Normalize();
        _movement *= MoveSpeed * Time.deltaTime;
    }

    void ApplyMovement()
    {
        transform.position += _movement;
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