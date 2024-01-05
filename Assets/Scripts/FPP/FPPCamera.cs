using UnityEngine;
using Cinemachine;
using UZSG.Systems;
using UZSG.Player;

namespace UZSG.FPP
{
    public class FPPCamera : MonoBehaviour
    {
        [Header("Camera Settings")]
        public float Sensitivity = 0.32f;
        public bool EnableControls = true;
        public bool EnableBobbing = true;

        [Header("Bob Settings")]
        public float Amplitude = 0.015f;
        public float Frequency = 15f;
        public float MinMoveSpeed = 3f;
        
        /// <summary>
        /// The original position of the camera.
        /// </summary>
        Vector3 _origin;
        float lerpTimer;
        
        [Header("Components")]
        [SerializeField] PlayerEntity player;
        [SerializeField] Camera mainCamera;
        [SerializeField] CinemachineVirtualCamera virtualCamera;
        [SerializeField] CinemachinePOV POV;

        void Awake()
        {
            mainCamera = Camera.main;
            virtualCamera = GetComponent<CinemachineVirtualCamera>();
            POV = virtualCamera.GetCinemachineComponent<CinemachinePOV>();
        }

        void Start()
        {
            _origin = virtualCamera.transform.localPosition;
        }

        void Update()
        {
            BobMotion();
            ResetPosition();
        }

        void BobMotion()
        {
            if (!EnableBobbing) return;
            if (!player.Controls.IsGrounded) return;
            if (player.Controls.HorizontalSpeed < MinMoveSpeed) return;

            Vector3 bobbing = Vector3.zero;
            float sine = Mathf.Sin(Time.time * Frequency);
            float cosine = Mathf.Cos(Time.time * Frequency / 2);

            bobbing += mainCamera.transform.right * (cosine * Amplitude * 2);
            bobbing += -mainCamera.transform.up * (sine * Amplitude);

            // virtualCamera.transform.localPosition = _origin + bobbing;

            /// This makes the bobbing speed per the TPS
            if (lerpTimer < Game.Tick.SecondsPerTick)
            {
                lerpTimer += Time.time;                    
                virtualCamera.transform.localPosition = Vector3.Lerp(_origin, bobbing, lerpTimer / Game.Tick.SecondsPerTick);
            } else
            {
                lerpTimer = 0f;
            }
        }
        
        void ResetPosition()
        {
            if (virtualCamera.transform.localPosition == _origin) return;

            virtualCamera.transform.localPosition = Vector3.Lerp(virtualCamera.transform.localPosition, _origin, 1f * Time.deltaTime);
        }

        public void ToggleBobbing(bool enabled)
        {

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