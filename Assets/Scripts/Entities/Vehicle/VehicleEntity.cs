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
        [Header("Vehicle Information")]
        [SerializeField] VehicleData vehicle; // vehicle data
        VehicleSeatManager _vehicleSeatManager;
        VehicleController _vehicleController;

        [Header("Vehicle Parts")]
        public GameObject Model;
        
        [Header("Vehicle Wheel Colliders")]
        public List<WheelCollider> FrontWheelColliders;
        public List<WheelCollider> RearWheelColliders;

        [Header("Vehicle Wheel Meshes")]
        public List<GameObject> WheelMeshes;

        public VehicleData Vehicle
        {
            get
            {
                return vehicle;
            }
            set
            {
                vehicle = value;
            }
        }

        public string Name => vehicle.Name;

        public string Action => "Drive";

        public event EventHandler<InteractArgs> OnInteract;

        int _originalLayer;

        protected virtual void Awake()
        {
            _vehicleController = gameObject.GetComponent<VehicleController>();
            _vehicleSeatManager = gameObject.GetComponent<VehicleSeatManager>();
            Model = transform.Find("Vehicle Body").gameObject;
            _originalLayer = Model.layer;
        }

        public void OnLookEnter()
        {
            if (Model != null && _vehicleSeatManager.Driver == null)
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

            _vehicleController.EnableVehicle();
            _vehicleSeatManager.EnterVehicle(player);
        }
    }
}

