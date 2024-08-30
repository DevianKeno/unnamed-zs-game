using System;
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

        #region This can be done better
        public BobSettings WalkBob = new();
        public BobSettings JogBob = new();
        public BobSettings RunBob = new();
        public BobSettings CrouchBob = new();
        #endregion
        
        [SerializeField] BobSettings _bobToUse;
        Vector3 _originalPosition; /// local
        Quaternion _originalRotation; /// local

        void Start()
        {
            _originalPosition = transform.localPosition;
            _originalRotation = transform.localRotation;
        }

        void Update()
        {
            if (!Enabled) return;

            Bob();
            RecoverPosition();
        }

        public void SetRunningBobSettings(ViewmodelSettings settings)
        {
            RunBob.RunningPosition = settings.RunBobPosition;
            RunBob.RunningRotation = settings.RunBobRotation;
        }

        void Bob()
        {
            if (!Player.Controls.IsGrounded) return;
            if (Player.Controls.Speed < MinSpeed) return;

            _bobToUse = GetBob();

            AddPosition(FootStepMotion());
            AddRunningGunRotationAdditive();
            if (_bobToUse.MaintainForwardLook)
            {
                transform.LookAt(FocusTarget());
            }
        }

        void AddRunningGunRotationAdditive()
        {
            Quaternion targetRotation = Quaternion.Inverse(transform.localRotation);
            if (Player.Controls.IsRunning)
            {
                targetRotation *= Quaternion.Euler(_bobToUse.RunningRotation);
            }
            else
            {
                targetRotation *= _originalRotation;
            }

            transform.localRotation = Quaternion.Slerp(
                transform.localRotation,
                targetRotation,
                _bobToUse.TransformDamping * Time.deltaTime
            );
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

        BobSettings GetBob()
        {
            if (Player.Controls.IsRunning)
            {
                return RunBob;
            }
            else if (Player.Controls.IsWalking)
            {
                return WalkBob;
            }
            else if (Player.Controls.IsCrouching)
            {
                return CrouchBob;
            }
            else
            {
                return JogBob;
            }
        }

    }
}
