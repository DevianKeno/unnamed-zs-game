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
    public struct VehicleInteractContext
    {
        public IInteractable Interactable { get; set; }
        public IInteractActor Actor { get; set; }
        /// <summary>
        /// This Entered/Exited bool can be goofy. May be an enum can be better.
        /// </summary>
        public bool Entered { get; set; }
        /// <summary>
        /// This Entered/Exited bool can be goofy. May be an enum can be better.
        /// </summary>
        public bool Exited { get; set; }
    }

    [RequireComponent(typeof(Rigidbody), typeof(VehicleController), typeof(VehicleFunctionAnimation))]
    public class VehicleEntity : Entity, IInteractable
    {
        [SerializeField] protected VehicleData vehicleData;
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
        public bool AllowInteractions { get; set; } = true;

        [field: Header("Vehicle Components")]
        public VehicleController Controller { get; private set; }
        public VehicleSeatManager SeatManager { get; private set; }
        public VehicleInputHandler InputHandler { get; private set; }
        public VehicleFunctionAnimation FunctionAnimation { get; private set; }
        public VehicleAudioManager AudioManager { get; private set; }
        public VehicleCameraManager CameraManager { get; private set; }
        public Camera TPPCamera;

        [Header("Vehicle Parts")]
        public GameObject Model;
        
        [Header("Vehicle Wheel Colliders")]
        public List<WheelCollider> FrontWheelColliders;
        public List<WheelCollider> RearWheelColliders;

        [Header("Vehicle Wheel Meshes")]
        public List<GameObject> WheelMeshes;

        public string DisplayName => vehicleData.Name;

        public string ActionText => "Drive";
        // {
        //     get
        //     {
        //         if (!HasDriver) return "Drive";
        //         return "Enter"
        //     }
        // }

        int _originalLayer;

        protected virtual void Awake()
        {
            Controller = gameObject.GetComponent<VehicleController>();
            SeatManager = gameObject.GetComponent<VehicleSeatManager>();
            InputHandler = gameObject.GetComponent<VehicleInputHandler>();
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

        public List<InteractAction> GetInteractActions()
        {
            var actions = new List<InteractAction>();
            string actionText;
            if (SeatManager.Driver == null)
            {
                actionText = "Drive";
            }
            else
            {
                actionText = "Enter";
            }
            actions.Add(new()
            {
                Interactable = this,
                ActionText = actionText,
                InteractableText = this.entityData.DisplayName,
                IsHold = true,
                InputAction = Game.Input.InteractPrimary,
            });

            return actions;
        }

        public void Interact(InteractionContext context)
        {
            if (context.Actor is not Player player) return;

            Controller.EnableVehicle();
            SeatManager.EnterVehicle(player);
            AudioManager.PlayerInVehicle();
        }
    }
}

