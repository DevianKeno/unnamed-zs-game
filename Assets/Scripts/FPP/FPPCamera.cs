using System;
using UnityEngine;
using Cinemachine;
using UZSG.Systems;
using UZSG.Entities;

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
        Vector3 _camOrigin;
        /// <summary>
        /// The original position of the weapon holder.
        /// </summary>
        public Vector3 WeaponOrigin;
        float lerpTimer;
        
        [Header("Components")]
        public Player player;
        public Transform WeaponHolder;
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
            _camOrigin = virtualCamera.transform.localPosition;
            Game.UI.OnCursorToggled += CursorToggledCallback;
        }

        void CursorToggledCallback(bool isVisible)
        {
            ToggleControls(!isVisible);
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

            Vector3 camBob = _camOrigin;
            Vector3 weaponBob = WeaponOrigin;
            float frequency = Time.time * Frequency;
            float sine = Mathf.Sin(frequency);
            float cosine = Mathf.Cos(frequency / 2);

            camBob += mainCamera.transform.right * (cosine * Amplitude * 2f);
            camBob += -mainCamera.transform.up * (sine * Amplitude);
            weaponBob += mainCamera.transform.right * (cosine * Amplitude * 0.5f);
            weaponBob += -mainCamera.transform.up * (sine * Amplitude * 0.5f);

            /// This makes the bobbing speed per the TPS
            if (lerpTimer < Game.Tick.SecondsPerTick)
            {
                lerpTimer += Time.time;                    
                virtualCamera.transform.localPosition = Vector3.Lerp(_camOrigin, camBob, lerpTimer / Game.Tick.SecondsPerTick);
                WeaponHolder.transform.localPosition = Vector3.Lerp(WeaponOrigin, weaponBob, lerpTimer / Game.Tick.SecondsPerTick);
            } else
            {
                lerpTimer = 0f;
            }
        }
        
        void ResetPosition()
        {
            if (virtualCamera.transform.localPosition == _camOrigin) return;

            virtualCamera.transform.localPosition = Vector3.Lerp(virtualCamera.transform.localPosition, _camOrigin, 1f * Time.deltaTime);
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