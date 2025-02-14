using System;
using UnityEngine;
using UZSG.Entities;

namespace UZSG.FPP
{
    /// <summary>
    /// Rotates the target transform to match the source camera rotation as it plays.
    /// </summary>
    public class FPPCameraAnimationTarget : MonoBehaviour
    {
        public Player Player;
        [Space]
        public bool Enabled;
        public float Slerp = 1f;
        bool _playAnimation;

        [SerializeField] FPPCameraInput cameraFpp;
        [SerializeField] FPPCameraAnimationSource source;
        public FPPCameraAnimationSource Source
        {
            get => source;
            set
            {
                Initialize(value);
            }
        }
        public Transform Target;
        Transform _cameraBone;
        readonly static Quaternion blenderOffset = Quaternion.Euler(180f, 0f, 0f);

        void LateUpdate()
        {
            if (!Enabled) return;

            ApplyRotation();
        }

        void Initialize(FPPCameraAnimationSource cas)
        {
            source = cas;

            if (source != null)
            {
                _cameraBone = source.CameraBone;
                Enabled = true;
            }
            else
            {
                _cameraBone = null;
                _playAnimation = false;
                Enabled = false;
            }
        }

        public void PlayAnimation()
        {
            Enabled = true;
            _playAnimation = true;
        }

        public void StopAnimation()
        {
            Enabled = false;
            _playAnimation = false;
        }

        void ApplyRotation()
        {
            if (_playAnimation)
            {
                Target.localRotation =  _cameraBone.localRotation * blenderOffset;
            }
        }
    }
}
