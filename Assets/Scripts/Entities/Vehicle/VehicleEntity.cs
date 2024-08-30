using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UZSG.Data;
using UZSG.Entities.Vehicles;
using UZSG.Interactions;
using UZSG.Systems;

namespace UZSG.Entities
{
    [RequireComponent(typeof(Rigidbody), typeof(VehicleController), typeof(VehicleFunctionAnimation))]
    public class VehicleEntity : Entity, IInteractable
    {
        public bool AllowInteractions { get; set; } = true;

        [Header("Vehicle Information")]
        [SerializeField] VehicleData vehicleData; // vehicle data
        public VehicleController Controller;
        public VehicleInputHandler InputHandler;
        public VehicleSeatManager SeatManager;
        public VehicleFunctionAnimation FunctionAnimation;
        public VehicleAudioManager AudioManager;
        public VehicleCameraManager CameraManager;

        [Header("Vehicle Parts")]
        public GameObject Model;
        
        [Header("Vehicle Wheel Colliders")]
        public List<WheelCollider> FrontWheelColliders;
        public List<WheelCollider> RearWheelColliders;

        [Header("Vehicle Wheel Meshes")]
        public List<GameObject> WheelMeshes;

        public Camera TPPCamera;
        
        public VehicleData VehicleData
        {
            get
            {
                return vehicleData;
            }
            set
            {
                vehicleData = value;
            }
        }

        public string Name => vehicleData.Name;

        public string Action => "Drive";

        public event EventHandler<InteractArgs> OnInteract;

        int _originalLayer;

        protected virtual void Awake()
        {
            Controller = gameObject.GetComponent<VehicleController>();
            InputHandler = gameObject.GetComponent<VehicleInputHandler>();
            SeatManager = gameObject.GetComponent<VehicleSeatManager>();
            FunctionAnimation = gameObject.GetComponent<VehicleFunctionAnimation>();
            AudioManager = gameObject.GetComponent<VehicleAudioManager>();
            CameraManager = gameObject.GetComponent<VehicleCameraManager>();
            Model = transform.Find("Vehicle Body").gameObject;
            _originalLayer = Model.layer;
        }

        public void OnLookEnter()
        {
            if (Model != null && SeatManager.Driver == null)
            {
                Model.layer = LayerMask.NameToLayer("Outline");
            }
        }

        public void OnLookExit()
        {
            if (Model != null)
            {
                Model.layer = _originalLayer;
            }
        }

        public void Interact(IInteractActor actor, InteractArgs args)
        {
            if (actor is not Player player) return;

            Controller.EnableVehicle();
            SeatManager.EnterVehicle(player);
            AudioManager.PlayerInVehicle();
        }
    }
}

