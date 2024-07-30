using UnityEngine;
using UZSG.Entities;

namespace UZSG.FPP
{
    public class FPPViewmodelBobbing : MonoBehaviour
    {
        public Player Player;
        [Space]

        [Header("Settings")]
        public bool Enabled = true;
        public float MinSpeed = 0.3f;
        public BobSettings WalkBob = new();
        public BobSettings RunBob = new();
        
        Vector3 _originalPosition;
        BobSettings _bobToUse;

        void Start()
        {
            _originalPosition = transform.localPosition;
        }

        void Update()
        {
            if (!Enabled) return;
            Bob();
            RecoverPosition();
        }

        void Bob()
        {
            if (Player.Controls.Speed < MinSpeed) return;
            if (!Player.Controls.IsGrounded) return;

            _bobToUse = Player.Controls.IsRunning ? RunBob : WalkBob;
            AddPosition(FootStepMotion());
            if (_bobToUse.MaintainForwardLook)
            {
                transform.LookAt(FocusTarget());
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

        void RecoverPosition()
        {
            Vector3 motion = Vector3.Lerp(
                transform.localPosition,
                _originalPosition,
                _bobToUse.Recovery * BobSettings.RecoveryFactor * Time.deltaTime) - transform.localPosition;
            AddPosition(motion);
        }
    }
}
