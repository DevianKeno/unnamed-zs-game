using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UZSG.Data;
using UZSG.Interactions;
using UZSG.Systems;

namespace UZSG.Entities
{
    public class VehicleEntity : Entity, IInteractable
    {
        [Header("Vehicle Information")]
        [SerializeField] VehicleData vehicle; // vehicle data

        [SerializeField] bool isFixed; // boolean for whether the vehicle is fixed (still not final)

        public Player Driver; // driver of the vehicle (still not final, will be moved to vehicle controller)

        #region Other Important Values
        InputAction backInput; // refers to the ESC button

        Transform playerParent;

        #endregion

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

        public string ActionText => "Drive";

        public event EventHandler<InteractArgs> OnInteract;

        int _originalLayer;
        GameObject model;
        Rigidbody rb;
        public Rigidbody Rigidbody => rb;

        protected virtual void Awake()
        {
            model = transform.Find("box").gameObject;
            rb = GetComponentInChildren<Rigidbody>();
            _originalLayer = model.layer;
        }

        private void Start()
        {
            backInput = Game.Main.GetInputAction("Back", "Global");
        }

        private void OnBackInputPerform(InputAction.CallbackContext obj)
        {
            ExitVehicle(Driver);
            backInput.performed -= OnBackInputPerform;
        }

        public void OnLookEnter()
        {
            if (model != null && Driver == null)
            {
                model.layer = LayerMask.NameToLayer("Outline");
            }
        }

        public void OnLookExit()
        {
            if (model != null)
            {
                model.layer = _originalLayer;
            }
        }

        public void Interact(IInteractActor actor, InteractArgs args)
        {
            if (actor is not Player player) return;

            EnterVehicle(player);
        }

        public void EnterVehicle(Player player)
        {
            // Check whether the vehicle is fixed to allow the player to enter the vehicle
            if (isFixed)
            {
                Driver = player; // Set player to driver
                model.ChangeTag("Untagged");
                player.Controls.Rigidbody.isKinematic = true; // "Disable" the Rigidbody
                player.Controls.Rigidbody.useGravity = false;
                foreach (CapsuleCollider collider in player.GetComponents<CapsuleCollider>()) // Disable the colliders
                {
                    collider.enabled = false;
                }
                playerParent = player.transform.parent;
                player.transform.SetParent(model.transform, false); // Set the player position to the parent
                player.transform.localPosition = new Vector3(0, -1, 1); // Fix the postion to the parent
                backInput.performed += OnBackInputPerform; // Subscribe function for exiting
            }
            // Output a Message instead
            else
            {
                Debug.Log("You cannot pass");
            }
        }

        public void ExitVehicle(Player player)
        {
            player.Controls.Rigidbody.isKinematic = false; // "Disable" the Rigidbody
            player.Controls.Rigidbody.useGravity = true;
            foreach (CapsuleCollider collider in player.GetComponents<CapsuleCollider>()) // Disable the colliders
            {
                collider.enabled = true;
            }

            player.transform.SetParent(playerParent, false); // Set the player position to the parent

            player.Controls.Rigidbody.position = new(model.transform.position.x + 2,
                                                     model.transform.position.y,
                                                     model.transform.position.z);

            player.Controls.Rigidbody.rotation = Quaternion.identity;
            model.ChangeTag("Interactable");
            Driver = null;
        }
    }
}

