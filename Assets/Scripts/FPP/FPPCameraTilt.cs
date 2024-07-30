using UnityEngine;
using UZSG.Entities;

namespace UZSG.FPP
{
    /// <summary>
    /// Adds tilt to the FPP Camera depending on the movement of the Player.
    /// </summary>
    public class FPPCameraTilt : MonoBehaviour
    {
        public Player Player;
        [Space]

        [Header("Settings")]
        public bool Enabled;
        public float MinSpeed;
        public float ForwardAngle;
        public float SideAngle;
        public float TiltSpeed;

        float _currentTiltX;
        float _currentTiltZ;
        float _targetTiltX;
        float _targetTiltZ;
        float _targetMultiplier;

        Quaternion _originalRotation;

        void Start()
        {
            _originalRotation = transform.localRotation;
        }

        void Update()
        {
            if (!Enabled) return;
            
            _targetMultiplier = Player.Controls.IsRunning ? 1.5f : 1f;
            HandleCameraTilt();
        }

        void HandleCameraTilt()
        {
            var localVelocity = transform.InverseTransformDirection(Player.Controls.Velocity);

            if (Mathf.Abs(localVelocity.z) < MinSpeed)
            {
                _targetTiltX = 0;
            }
            else if (localVelocity.z > MinSpeed) /// Moving forward
            {
                _targetTiltX = ForwardAngle;
            }
            else if (localVelocity.z < MinSpeed) /// Moving backwards
            {
                _targetTiltX = -ForwardAngle;
            }
            
            if (Mathf.Abs(localVelocity.x) < MinSpeed) 
            {
                _targetTiltZ = 0;
            }
            else if (localVelocity.x < MinSpeed) /// Moving left
            {
                _targetTiltZ = SideAngle;
            }
            else if (localVelocity.x > MinSpeed) /// Moving right
            {
                _targetTiltZ = -SideAngle;
            }
            
            _currentTiltX = Mathf.Lerp(_currentTiltX, _targetTiltX * _targetMultiplier, Time.deltaTime * TiltSpeed);
            _currentTiltZ = Mathf.Lerp(_currentTiltZ, _targetTiltZ * _targetMultiplier, Time.deltaTime * TiltSpeed);
            transform.localRotation = Quaternion.Euler(_currentTiltX, 0, _currentTiltZ);
        }
    }
}