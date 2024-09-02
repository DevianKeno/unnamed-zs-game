using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace UZSG.Entities.Vehicles
{
    public class VehicleCameraManager : MonoBehaviour
    {
        VehicleEntity _vehicle;
        Transform _vehicleCameraParent;

        [Header("Vehicle Camera Transforms")]
        public Transform TPPCameraView;

        private void Awake()
        {
            _vehicle = GetComponent<VehicleEntity>();
            _vehicleCameraParent = TPPCameraView.transform;
        }

        private void Start()
        {
            _vehicle.TPPCamera.gameObject.SetActive(false);
        }

        private void LateUpdate()
        {
            /*if (_vehicle.TPPCamera.gameObject.activeSelf == false) return;

            _vehicle.TPPCamera.transform.position = Vector3.Lerp(
                _vehicle.TPPCamera.transform.position,
                TPPCameraView.transform.position,
                0.1f
            );

            _vehicle.TPPCamera.transform.rotation = Quaternion.Lerp(
                _vehicle.TPPCamera.transform.rotation,
                TPPCameraView.transform.rotation,
                0.1f
            );*/

            _vehicle.TPPCamera.transform.LookAt(_vehicle.Model.transform);
        }


        /// <summary>
        /// Changes the Camera View from TPP to FPP
        /// </summary>
        /// <param name="player"></param>
        public void ChangeVehicleView(Player player)
        {
            if (player.MainCamera.gameObject.activeSelf == false)
            {
                ToggleCamera(player, "FPP");
            }
            else
            {
                ToggleCamera(player, "TPP");
            }
        }

        /// <summary>
        /// Responsible for toggling the Camera on and off as well as calling the Input Enable/Disable. Can either be "FPP" of "TPP".
        /// </summary>
        /// <param name="player"></param>
        /// <param name="viewPerspective"></param>
        public void ToggleCamera(Player player, string viewPerspective)
        {
            if (viewPerspective == "FPP")
            {
                player.MainCamera.gameObject.SetActive(true);
                _vehicle.TPPCamera.gameObject.SetActive(false);
                _vehicle.InputHandler.ToggleFPPCameraInput(player, true);
                SetTPPCameraPosition(false);
            }
            else if (viewPerspective == "TPP")
            {
                player.MainCamera.gameObject.SetActive(false);
                _vehicle.TPPCamera.gameObject.SetActive(true);
                _vehicle.InputHandler.ToggleFPPCameraInput(player, false);
                SetTPPCameraPosition(true);
            }
        }

        /// <summary>
        /// Sets the TPP Camera Postion.
        /// </summary>
        /// <param name="setPosition"></param>
        public void SetTPPCameraPosition(bool inUse)
        {
            if (inUse)
            {
                _vehicle.TPPCamera.transform.SetParent(_vehicleCameraParent);
                _vehicle.TPPCamera.transform.localPosition = Vector3.zero;
            }
            else
            {
                _vehicle.TPPCamera.transform.SetParent(_vehicle.Model.transform, false);
                _vehicle.TPPCamera.transform.localPosition = Vector3.zero;
                _vehicle.TPPCamera.transform.localRotation = Quaternion.identity;
            }
        }
    }
}

