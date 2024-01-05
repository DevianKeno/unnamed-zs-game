using UnityEngine;
using Cinemachine;
using UZSG.Systems;
using UnityEngine.InputSystem;
using System;

namespace UZSG
{
    public class FreeLookCamera : MonoBehaviour
    {
        public bool EnableControls = true;
        public float MoveSpeed = 5f;
        public float Sensitivity = 0.32f;

        Vector3 _movement;

        [SerializeField] Camera mainCamera;
        [SerializeField] CinemachineVirtualCamera virtualCamera;
        CinemachinePOV POV;
        [SerializeField] PlayerInput input;
        InputAction moveInput;
        InputAction jumpInput;
        InputAction crouchInput;
        InputAction runInput;

        void Awake()
        {
            mainCamera = Camera.main;
            virtualCamera = GetComponent<CinemachineVirtualCamera>();
            POV = virtualCamera.GetCinemachineComponent<CinemachinePOV>();
            input = GetComponent<PlayerInput>();            
            
            moveInput = input.actions.FindAction("Move");
            jumpInput = input.actions.FindAction("Jump");
            runInput = input.actions.FindAction("Run");
            crouchInput = input.actions.FindAction("Crouch");
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
            Vector3 movement = (input.x * mainCamera.transform.right) + (input.y * mainCamera.transform.forward);

            transform.position += movement * (MoveSpeed * Time.deltaTime);
        }

        void ConsoleWindowToggledCallback(bool isVisible)
        {
            ToggleControls(!isVisible);
        }

        public void Initialize()
        {
            Game.UI.OnCursorToggled += CursorToggledCallback;            
            Game.UI.ConsoleUI.OnToggle += ConsoleWindowToggledCallback;
            
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

        public void ToggleControls()
        {
            ToggleControls(!EnableControls);
        }

        public void ToggleControls(bool enabled)
        {
            EnableControls = enabled;

            if (enabled)
            {
                POV.m_VerticalAxis.m_MaxSpeed = Sensitivity;
                POV.m_HorizontalAxis.m_MaxSpeed = Sensitivity;
            } else
            {
                POV.m_VerticalAxis.m_MaxSpeed = 0f;
                POV.m_HorizontalAxis.m_MaxSpeed = 0f;
            }
        }
    }
}
