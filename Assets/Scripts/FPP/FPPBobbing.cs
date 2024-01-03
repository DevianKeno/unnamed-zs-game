using System;
using UnityEngine;
using UZSG.Systems;

namespace UZSG.Player
{
    public class FPPBobbing : MonoBehaviour
    {
        public bool IsEnabled = true;
        public float Amplitude = 0.015f;
        public float Frequency = 15f;
        public float MinMoveSpeed = 3f;
        Vector3 _startPosition;
        [SerializeField] PlayerControls controls;
        [SerializeField] PlayerEntity player;
        [SerializeField] Transform model;
        [SerializeField] Transform virtualCamera;

        void Start()
        {
            _startPosition = virtualCamera.localPosition;
            SetEnabled(true);
        }

        void Tick(object sender, TickEventArgs e)
        {
            if (!IsEnabled) return;

            CheckMotion();
            ResetPosition();
        }

        public void SetEnabled(bool value)
        {
            IsEnabled = value;
            
            if (value)
            {
                Game.Tick.OnTick += Tick;
            } else
            {
                Game.Tick.OnTick -= Tick;
            }
        }

        void CheckMotion()
        {
            if (!controls.IsGrounded) return;
            if (controls.HorizontalSpeed < MinMoveSpeed) return;

            Vector3 bobbing = Vector3.zero;
            float sine = Mathf.Sin(Time.time * Frequency);
            float cosine = Mathf.Cos(Time.time * Frequency / 2);

            bobbing += player.MainCamera.transform.right * (cosine * Amplitude * 2);
            bobbing += -player.MainCamera.transform.up * (sine * Amplitude);

            virtualCamera.localPosition = _startPosition + bobbing;
        }

        void ResetPosition()
        {
            if (virtualCamera.localPosition == _startPosition) return;

            virtualCamera.localPosition = Vector3.Lerp(virtualCamera.localPosition, _startPosition, 0.2f * Time.deltaTime);
        }
    }
}   
