using System;
using System.Collections.Generic;
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
        [SerializeField] ViewmodelBobPreset viewmodelBobToUse;

        ViewmodelSettings _currentViewmodelSettings;
        Dictionary<string, ViewmodelBobPreset> _cachedViewmodelBobPresets = new();
        
        bool _hasValidPreset;
        Vector3 _originalLocalPosition;
        Quaternion _originalLocalRotation;

        void Start()
        {
            _originalLocalPosition = transform.localPosition;
            _originalLocalRotation = transform.localRotation;
        }

        void Update()
        {
            if (!Enabled) return;

            Bob();
            RecoverPosition();
        }


        #region Public methods

        public void SetViewmodelSettings(ViewmodelSettings settings)
        {
            _currentViewmodelSettings = settings;
            
            /// cache values
            foreach (var preset in _currentViewmodelSettings.BobbingPresets)
            {
                _cachedViewmodelBobPresets[preset.Id] = preset;
            }
        }

        #endregion


        void Bob()
        {
            if (!Player.Controls.CanBob) return;

            viewmodelBobToUse = GetViewmodelBob();
            
            AddPosition(CalculateFootStepMotion()); /// inherent bobbing motion

            /// add running gun stance motion additive cumulative speculative superlative
            AddRunningGunPositionAdditive();
            AddRunningGunRotationAdditive();

            if (viewmodelBobToUse.BobSettings.MaintainForwardLook)
            {
                transform.LookAt(FocusTarget());
            }
        }

        void AddRunningGunRotationAdditive()
        {
            Quaternion targetRotation;
            if (_hasValidPreset)
            {
                targetRotation = Quaternion.Inverse(transform.localRotation);
                if (Player.Controls.IsRunning)
                {
                    targetRotation *= Quaternion.Euler(viewmodelBobToUse.Rotation);
                }
                else
                {
                    targetRotation *= _originalLocalRotation;
                }
            }
            else
            {
                targetRotation = _originalLocalRotation;
            }

            transform.localRotation = Quaternion.Slerp(
                transform.localRotation,
                targetRotation,
                viewmodelBobToUse.Damping * Time.deltaTime
            );
        }

        void AddRunningGunPositionAdditive()
        {
            Vector3 targetPosition;
            if (_hasValidPreset)
            {
                targetPosition = viewmodelBobToUse.Position;
            }
            else
            {
                targetPosition = _originalLocalPosition;
            }

            Vector3 motion = Vector3.Lerp(
                transform.localPosition,
                targetPosition,
                viewmodelBobToUse.Damping * Time.deltaTime
            );

            AddPosition(motion - transform.localPosition);
        }

        void AddPosition(Vector3 motion)
        {
            transform.localPosition += motion;
        }

        Vector3 CalculateFootStepMotion()
        {
            var position = Vector2.zero;
            var amplitude = viewmodelBobToUse.BobSettings.Amplitude * FPP.BobSettings.AmplitudeFactor;
            var frequency = viewmodelBobToUse.BobSettings.Frequency * FPP.BobSettings.FrequencyFactor;

            position.x += Mathf.Cos(Time.time * frequency / 2) * amplitude * 2;
            position.y += Mathf.Sin(Time.time * frequency) * amplitude;
            return position;
        }

        Vector3 FocusTarget()
        {
            return transform.position + Player.Forward * viewmodelBobToUse.BobSettings.LookDistance;
        }

        void RecoverPosition()
        {
            Vector3 motion = Vector3.Lerp(
                transform.localPosition,
                _originalLocalPosition,
                viewmodelBobToUse.BobSettings.Recovery * FPP.BobSettings.RecoveryFactor * Time.deltaTime
            );

            AddPosition(motion - transform.localPosition);
        }

        ViewmodelBobPreset GetViewmodelBob()
        {
            if (_cachedViewmodelBobPresets.TryGetValue(GetIdFromPlayerState(), out ViewmodelBobPreset preset))
            {
                _hasValidPreset = true;
                return preset;
            }
            else
            {
                _hasValidPreset = false;
                return new(); /// zero values
            }
        }

        string GetIdFromPlayerState()
        {
            if (Player.Controls.IsRunning)
            {
                return "run";
            }
            else if (Player.Controls.IsWalking)
            {
                return "walk";
            }
            else if (Player.Controls.IsCrouching)
            {
                return "crouch";
            }
            else
            {
                return "jog";
            }
        }
    }
}
