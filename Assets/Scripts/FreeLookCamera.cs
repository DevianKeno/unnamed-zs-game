using UnityEngine;
using Cinemachine;

namespace UZSG
{
    public class FreeLookCamera : MonoBehaviour
    {
        public bool EnableControls = true;
        public float MoveSpeed = 5f;
        public float Sensitivity = 0.32f;

        [SerializeField] Camera mainCamera;
        [SerializeField] CinemachineVirtualCamera virtualCamera;
        CinemachinePOV POV;

        void Awake()
        {
            mainCamera = Camera.main;
            virtualCamera = GetComponent<CinemachineVirtualCamera>();
            POV = virtualCamera.GetCinemachineComponent<CinemachinePOV>();
        }

        void Update()
        {
            if (!EnableControls) return;

            // Get movement input
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");
            Vector3 movement = (horizontal * mainCamera.transform.right) + (vertical * mainCamera.transform.forward);
            // movement.y = 0f; // Prevent movement on the y-axis (no flying)

            transform.position += movement * (MoveSpeed * Time.deltaTime);

            // Get mouse input for camera rotation
            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");

            if (POV != null)
            {
                POV.m_HorizontalAxis.Value += mouseX * Sensitivity;
                POV.m_VerticalAxis.Value -= mouseY * Sensitivity;
            }
        }

        public void ToggleControls(bool enabled)
        {
            EnableControls = enabled;

            if (enabled)
            {
                POV.m_VerticalAxis.m_MaxSpeed = Sensitivity;
                POV.m_HorizontalAxis.m_MaxSpeed = Sensitivity;
            }
            else
            {
                POV.m_VerticalAxis.m_MaxSpeed = 0f;
                POV.m_HorizontalAxis.m_MaxSpeed = 0f;
            }
        }
    }
}
