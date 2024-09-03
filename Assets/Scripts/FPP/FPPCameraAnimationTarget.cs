using System;
using UnityEngine;
using UZSG.Entities;

namespace UZSG.FPP
{
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

        void ApplyRotation() /// additively
        {
            if (_playAnimation)
            {
                var targetRotation = Quaternion.Inverse(cameraFpp.transform.rotation) * _cameraBone.rotation;
                Target.localRotation = Quaternion.Slerp(Target.localRotation, targetRotation, Slerp * Time.deltaTime);
            }
            // else
            // {
            //     Target.localRotation *= Quaternion.Slerp(Target.localRotation, Quaternion.identity, Slerp / Time.deltaTime);
            
            //     print($"EVALLL {Target.localRotation} == {Quaternion.identity}");
            //     if (Target.localRotation == Quaternion.identity)
            //     {
            //         print($"STOPPING ANIM! {Target.localRotation} == {Quaternion.identity}");
            //         StopAnimation();
            //         Enabled = false;
            //     }
            // }
        }
    }
}
