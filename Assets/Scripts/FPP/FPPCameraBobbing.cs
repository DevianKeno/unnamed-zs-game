using UnityEngine;
using UZSG.Entities;

namespace UZSG.FPP
{
    public class FPPCameraBobbing : MonoBehaviour
    {
        public Player Player;
        [Space]

        [Header("Settings")]
        public bool Enabled = true;
        public float MinSpeed = 0.3f;
        public BobSettings WalkBob = new();
        public BobSettings RunBob = new();
        public BobSettings CrouchBob = new();
        [SerializeField] BobSettings _bobToUse;

        Vector3 _originalPosition;
        
        void Start()
        {
            _originalPosition = transform.localPosition;
        }

        void Update()
        {
            if (!Enabled) return;
            Bob();
            ResetPosition();
        }

        void Bob()
        {
            if (!Player.Controls.IsGrounded) return;
            if (Player.Controls.HorizontalSpeed < MinSpeed) return;

            _bobToUse = setBob();

            AddPosition(FootStepMotion());
            if (_bobToUse.MaintainForwardLook)
            {
                transform.LookAt(FocusTarget());
            }
        }

        BobSettings setBob()
        {
            if (Player.Controls.IsRunning)
            {
                return RunBob;
            }
            else if (Player.Controls.IsCrouching)
            {
                return CrouchBob;
            }
            else
            {
                return WalkBob;
            }
        }

        void AddPosition(Vector3 motion)
        {
            transform.localPosition += motion;
        }

        Vector3 FootStepMotion()
        {
            var pos = Vector3.zero;
            var amplitude = _bobToUse.Amplitude * BobSettings.AmplitudeFactor;
            var frequency = _bobToUse.Frequency * BobSettings.FrequencyFactor;
            
            pos.x += Mathf.Cos(Time.time * frequency / 2) * amplitude * 2;
            pos.y += Mathf.Sin(Time.time * frequency) * amplitude;
            return pos;
        }

        Vector3 FocusTarget()
        {
            return transform.position + Player.Forward * _bobToUse.LookDistance;
        }

        void ResetPosition()
        {
            if (transform.localPosition == Vector3.zero) return;

            transform.localPosition = Vector3.Lerp(
                transform.localPosition,
                _originalPosition,
                _bobToUse.Recovery * BobSettings.RecoveryFactor * Time.deltaTime
            );
        }
    }
}

