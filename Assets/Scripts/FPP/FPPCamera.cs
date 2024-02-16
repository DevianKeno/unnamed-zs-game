using System;
using UnityEngine;
using Cinemachine;
using UZSG.Systems;
using UZSG.Entities;

namespace UZSG.FPP
{
    public class FPPCamera : MonoBehaviour
    {
        [Header("Camera Settings")]
        public float Sensitivity = 0.32f;
        public bool EnableControls = true;
        public bool EnableBobbing = true;

        [Header("Bob Settings")]
        public float Amplitude = 0.005f;
        public float Frequency = 15f;
        public float MinMoveSpeed = 3f;

        public abstract class BobSetting
        {
            public abstract float Amplitude { get; }
            public abstract float Frequency { get; }
        }

        class ForwardBobSettings : BobSetting
        {
            public override float Amplitude => 0.005f;
            public override float Frequency => 15f;
        }

        class BackwardBobSettings : BobSetting
        {
            public override float Amplitude => 0.005f;
            public override float Frequency => 15f;
        }

        class SidewardBobSettings : BobSetting
        {
            public override float Amplitude => 0.005f;
            public override float Frequency => 15f;
        }
        
        /// <summary>
        /// The original position of the camera.
        /// </summary>
        Vector3 _origin;
        /// <summary>
        /// The original position of the weapon holder.
        /// </summary>
        public Vector3 WeaponOrigin;
        float bobLerpTimer;
        float camLerpTimer;
        
        [Header("Components")]
        public Player Player;
        public Transform FPPModelHolder;
        [SerializeField] CinemachineVirtualCamera virtualCamera;

        [Header("Runtime Components")]
        public FPPModel FPPModel;
        [SerializeField] Camera mainCamera;
        // [SerializeField] CinemachinePOV POV;

        void Awake()
        {
            mainCamera = Camera.main;
            virtualCamera = GetComponent<CinemachineVirtualCamera>();
            
            WeaponOrigin = FPPModelHolder.transform.localPosition;
        }

        void Start()
        {
            // POV = virtualCamera.GetCinemachineComponent<CinemachinePOV>();
            _origin = transform.localPosition;
            Game.UI.OnCursorToggled += CursorToggledCallback;
        }

        void CursorToggledCallback(bool isVisible)
        {
            // ToggleControls(!isVisible);
        }

        float locLerpTimer;
        float rotLerpTimer;

        Transform cur;
        Transform target;

        Vector3 _framePosition;
        Quaternion _frameRotation;

        class CameraMotion
        {
            
        }

        void Update()
        {
            // _framePosition = Vector3.zero;
            // _frameRotation = Quaternion.identity;
            
            ApplyForwardBob();
            // ApplyCameraAnims();
            // ResetPosition();

            HandlePosition();
            HandleRotation();
            // ApplyMotion();
        }

        void HandlePosition()
        {
            
        }

        void HandleRotation()
        {

        }

        void ApplyMotion()
        {
            Vector3 target = transform.localPosition + (_framePosition * Time.fixedDeltaTime);

            if (transform.localPosition != target)
            {
                Vector3 displacement = target - transform.localPosition;
                transform.localPosition = displacement / Game.Tick.SecondsPerTick; // Will break if SecondsPerTick is 0
            }
        }

        public void LocMotion(Vector3 motion)
        {
            _framePosition += motion;
        }

        public void RotMotion(Quaternion motion)
        {
            var a = _frameRotation;
            _frameRotation = new(
                a.x + motion.x,
                a.y + motion.y,
                a.z + motion.z,
                a.w + motion.w
            );
        }

        public void SetBob(BobSetting b)
        {
            Amplitude = b.Amplitude;
            Frequency = b.Frequency;
        }

        void ApplyForwardBob()
        {
            if (!EnableBobbing) return;
            if (!Player.Controls.IsGrounded) return;
            if (Player.Controls.HorizontalSpeed < MinMoveSpeed) return;

            Vector3 camBob = _origin;
            Vector3 weaponBob = WeaponOrigin;
            float frequency = Time.time * Frequency;
            float sine = Mathf.Sin(frequency);
            float cosine = Mathf.Cos(frequency / 2);

            camBob += mainCamera.transform.right * (cosine * Amplitude * 2f);
            camBob += mainCamera.transform.up * (sine * Amplitude);
            // weaponBob += mainCamera.transform.right * (cosine * Amplitude * 0.5f);
            // weaponBob += -mainCamera.transform.up * (sine * Amplitude * 0.5f);
            
            // _framePosition += camBob;
            LocMotion(camBob);

            /// This makes the bobbing speed per the TPS
            // if (bobLerpTimer < Game.Tick.SecondsPerTick)
            // {
            //     bobLerpTimer += Time.time;                    
            //     virtualCamera.transform.localPosition = Vector3.Lerp(_camOrigin, camBob, bobLerpTimer / Game.Tick.SecondsPerTick);
            //     // WeaponHolder.transform.localPosition = Vector3.Lerp(WeaponOrigin, weaponBob, bobLerpTimer / Game.Tick.SecondsPerTick);
            // } else
            // {
            //     bobLerpTimer = 0f;
            // }
        }
        
        void ResetPosition()
        {
            if (transform.localPosition == _origin) return;

            transform.localPosition = Vector3.Lerp(transform.localPosition, _origin, 1f * Time.deltaTime);
        }

        void ApplyCameraAnims()
        {
            // if (FPPModel == null) return;
            // /// This makes the camera animation speed per the TPS
            // if (camLerpTimer < Game.Tick.SecondsPerTick)
            // {
            //     camLerpTimer += Time.time;

            //     /// This is fucked up idk, coulda used some help                
            //     var source = FPPModel.CameraAnims.transform.rotation; 
            //     var origin = virtualCamera.transform.parent.transform.localRotation;

            //     var x = source.x - initialCamRot.x;
            //     var y = source.y - initialCamRot.y;
            //     var z = source.z - initialCamRot.z;
            //     var w = source.w - initialCamRot.w;

            //     Quaternion target = new(
            //         origin.x + x,
            //         origin.y + y,
            //         origin.z + z,
            //         origin.w + w
            //     );

            //     virtualCamera.transform.parent.transform.localRotation = Quaternion.Lerp(
            //         origin,
            //         target,
            //         camLerpTimer / Game.Tick.SecondsPerTick
            //     );

            // } else
            // {
            //     camLerpTimer = 0f;
            // }
        }

        Quaternion initialCamRot;
        public void LoadModel(FPPModel model)
        {
            FPPModel = model;
            // initialCamRot = model.CameraAnims.transform.localRotation;
        }

        public void ToggleBobbing(bool enabled)
        {

        }

        // public void ToggleControls()
        // {
        //     ToggleControls(!EnableControls);
        // }

        // public void ToggleControls(bool enabled)
        // {
        //     EnableControls = enabled;

        //     if (enabled)
        //     {
        //         POV.m_VerticalAxis.m_MaxSpeed = Sensitivity;
        //         POV.m_HorizontalAxis.m_MaxSpeed = Sensitivity;
        //     } else
        //     {
        //         POV.m_VerticalAxis.m_MaxSpeed = 0f;
        //         POV.m_HorizontalAxis.m_MaxSpeed = 0f;
        //     }
        // }
    }
}