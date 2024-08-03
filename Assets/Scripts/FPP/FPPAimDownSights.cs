using UnityEngine;
using UZSG.Systems;

namespace UZSG.FPP
{
    public class FPPAimDownSights : MonoBehaviour
    {
        public bool _isAimingDown;
        public Transform FPPCamera;
        public Transform gun;
        public Transform frontSight;
        public Transform rearSight;
        public float adsSpeed = 5.0f; // Speed of transitioning to ADS

        Vector3 originalPosition;
        Quaternion originalRotation;
        
        void Start()
        {
            originalPosition = gun.localPosition;
            originalRotation = gun.localRotation;
        }

        public void AimDownSights()
        {
            _isAimingDown = !_isAimingDown;
            if (_isAimingDown)
            {
                RotateViewmodel();
            }
            else
            {
                ReturnToNormal();
            }
        }

        public void RotateViewmodel()
        {
            print("ROTAING!!");
            /// Calculate the direction from the rear sight to the camera's center
            Vector3 rearSightToCamera = (FPPCamera.position - rearSight.position).normalized;
            /// Calculate the desired rotation to align the rear sight with the camera's forward direction
            Quaternion targetRotation = Quaternion.LookRotation(rearSightToCamera);
            
            var frontOffset = gun.position - frontSight.position;
            var rearOffset = gun.position - rearSight.position;

            // Adjust the weapon's position to account for the pivot offset
            Vector3 targetPosition = FPPCamera.position + FPPCamera.forward * rearOffset.magnitude;

            // Move the weapon to align the rear sight with the camera's center
            // gun.localRotation = Quaternion.Slerp(gun.localRotation, targetRotation, adsSpeed * Time.deltaTime);
            // gun.localPosition = Vector3.Lerp(gun.localPosition, targetPosition, adsSpeed * Time.deltaTime);
    
        }

        public void ReturnToNormal()
        {
            print("REUTRNING!!");
            // Return the weapon to its original position and rotation
            // gun.localPosition = Vector3.Lerp(gun.localPosition, originalPosition, adsSpeed * Time.deltaTime);
            // gun.localRotation = Quaternion.Slerp(gun.localRotation, originalRotation, adsSpeed * Time.deltaTime);
        }
    }
}