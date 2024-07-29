using UnityEngine;

using UZSG.Systems;
using UZSG.Entities;

namespace UZSG.FPP
{
    public class FPPViewmodelBobbing : MonoBehaviour
    {
        public const float NormalAmplitudeFactor = 0.005f;
        public const float NormalFrequencyFactor = 20;
        public const float NormalRecoveryFactor = 2f;

        public Player Player;
        [Space]

        [Header("Bob Settings")]
        public bool Enabled = true;
        public float MinSpeed = 0.3f;
        public float Amplitude = 1f;
        public float Frequency = 1f;
        public float Recovery = 1f;
        public bool MaintainForwardLook;
        public float LookDistance = 16f;
        
        Vector3 _initialPosition;
        
        void Start()
        {
            _initialPosition = transform.localPosition;
            Game.Tick.OnTick += Tick;
        }

        void Tick(TickInfo info)
        {
            if (!Enabled) return;
            Bob();
            RecoverPosition();
        }

        void Bob()
        {
            if (Player.Controls.Speed < MinSpeed) return;
            if (!Player.Controls.IsGrounded) return;

            AddPosition(FootStepMotion());
            if (MaintainForwardLook)
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
            var frequency = Frequency * NormalFrequencyFactor;
            var amplitude = Amplitude * NormalAmplitudeFactor;

            pos.x += Mathf.Cos(Time.time * frequency / 2) * amplitude * 2;
            pos.y += Mathf.Sin(Time.time * frequency) * amplitude;
            return pos;
        }

        Vector3 FocusTarget()
        {
            return transform.position + Player.Forward * LookDistance;
        }

        void RecoverPosition()
        {
            Vector3 motion = Vector3.Lerp(transform.localPosition, _initialPosition, Recovery * NormalRecoveryFactor * Time.deltaTime) - transform.localPosition;
            AddPosition(motion);
        }
    }
}
