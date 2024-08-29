using UnityEngine;
using UZSG.Entities;

namespace UZSG.FPP
{
    public class FPPViewmodelMovement : MonoBehaviour
    {
        public Player Player;
        [Space]
        
        [Header("Settings")]
        public bool Enabled;
        public float Amount = 0.1f;
        public float Speed = 2f;

        Vector3 _initialPosition;
        Vector3 _targetOffset;

        void Start()
        {
            _initialPosition = transform.localPosition;
        }

        void Update()
        {
            if (!Enabled) return;
            HandleViewmodelMovement();
        }

        void HandleViewmodelMovement()
        {
            var localVelocity = transform.InverseTransformDirection(Player.Controls.Velocity);

            _targetOffset = Vector3.zero;

            if (localVelocity.z > 0.1f) /// Moving forward
            {
                _targetOffset += Vector3.forward * Amount;
            }
            else if (localVelocity.z < -0.1f) /// Moving backward
            {
                _targetOffset += Vector3.back * Amount;
            }

            if (localVelocity.x > 0.1f) /// Moving right
            {
                _targetOffset += Vector3.right * Amount;
            }
            else if (localVelocity.x < -0.1f) /// Moving left
            {
                _targetOffset += Vector3.left * Amount;
            }

            transform.localPosition = Vector3.Lerp(
                transform.localPosition,
                _initialPosition + _targetOffset,
                Time.deltaTime * Speed);
        }
    }
}