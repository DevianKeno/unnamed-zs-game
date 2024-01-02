using UnityEngine;
using Cinemachine;
using UZSG.Systems;
using UnityEngine.InputSystem;

namespace UZSG
{
    public class FreeLookCamera : MonoBehaviour
    {
        public Camera MainCamera;
        public CinemachineVirtualCamera VirtualCamera;
        public bool EnableControls = true;
        public float MoveSpeed = 5f;
        public float Sensitivity = 2f;
        float rotationX = 0f;
        float rotationY = 0f;

        [SerializeField] PlayerInput input;
        InputAction moveInput;
        InputAction jumpInput;
        InputAction crouchInput;
        InputAction runInput;

        void Awake()
        {
            VirtualCamera = GetComponent<CinemachineVirtualCamera>();
            input = GetComponent<PlayerInput>();
        }

        void Start()
        {
            Initialize();
        }

        void OnDisable()
        {
            Game.UI.OnCursorToggled -= CursorToggledCallback;
        }

        void CursorToggledCallback(bool isVisible)
        {
            EnableControls = !isVisible;
        }

        void Update()
        {
            Vector2 input = moveInput.ReadValue<Vector2>();
            Vector3 movement = (input.x * MainCamera.transform.right) + (input.y * MainCamera.transform.forward);

            Quaternion dRotation = Quaternion.Euler(MainCamera.transform.eulerAngles.x, 0f, 0f);
            MainCamera.transform.rotation = Quaternion.Slerp(transform.rotation, dRotation, Time.deltaTime * Sensitivity);

            transform.position += movement * (MoveSpeed * Time.deltaTime);
        }

        public void Initialize()
        {
            Game.UI.OnCursorToggled += CursorToggledCallback;
            
            moveInput = input.actions.FindAction("Move");
            jumpInput = input.actions.FindAction("Jump");
            runInput = input.actions.FindAction("Run");
            crouchInput = input.actions.FindAction("Crouch");
            
            moveInput.Enable();
            jumpInput.Enable();
            runInput.Enable();
            crouchInput.Enable();

            runInput.started += (context) =>
            {
                MoveSpeed = 10f;
            };

            runInput.canceled += (context) =>
            {
                MoveSpeed = 5f;
            };
        }

    }
}
