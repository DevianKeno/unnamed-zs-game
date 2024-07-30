using System;
using UnityEngine;

namespace UZSG.FPP
{
    public class FPPCameraAnimationTarget : MonoBehaviour
    {
        [SerializeField] bool _enabled = false;
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

        void Update()
        {
            if (!_enabled) return;

            ApplyRotation();
        }

        void Initialize(FPPCameraAnimationSource cas)
        {
            source = cas;

            if (source != null)
            {
                _cameraBone = source.CameraBone;
                _previousRotation = _cameraBone.rotation;
                _enabled = true;
            }
            else
            {
                _cameraBone = null;
                _enabled = false;
            }
        }

        public void Enable()
        {
            _enabled = true;
        }

        public void Disable()
        {
            _enabled = false;
        }

        Quaternion _previousRotation;

        void ApplyRotation()
        {
            Target.rotation = _cameraBone.rotation;
            // Target.rotation = Quaternion.Slerp(
            //     _previousRotation,
            //     _cameraBone.rotation,
            //     1 / Time.deltaTime
            // );
            // _previousRotation = Target.rotation;
        }
    }
}
