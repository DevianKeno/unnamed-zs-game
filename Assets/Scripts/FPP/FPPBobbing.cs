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
                IsEnabled = false;
                Game.Tick.OnTick -= Tick;
            }
        }

        void CheckMotion()
        {
            if (!controls.IsGrounded) return;
            if (controls.HorizontalSpeed < MinMoveSpeed) return;
            
            Vector3 pos = Vector3.zero;
            pos.y += Mathf.Sin(Time.time * Frequency) * Amplitude;
            pos.x = Mathf.Cos(Time.time * Frequency / 2) * Amplitude * 2;

            transform.localPosition += pos;
        }

        void ResetPosition()
        {
            if (virtualCamera.localPosition == _startPosition) return;

            virtualCamera.localPosition = Vector3.Lerp(virtualCamera.localPosition, _startPosition, 1 * Time.deltaTime);
        }
    }
}   
