using System;
using System.Collections.Generic;
using System.Linq;
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

        #region Vehicle Seats
        public Player Driver; // driver of the vehicle
        public List<Player> Passengers; // passenger of the vehicle
        public List<Transform> Seats; // transform of the seats of the vehicle
        #endregion

        #region Other Important Values
        InputAction backInput; // refers to the ESC button
        InputAction switchInput; // refers to the F button when switching seats
        Transform playerParent; // refers to the parent of the player in the game world
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
            model = transform.Find("Vehicle Body").gameObject;
            rb = GetComponentInChildren<Rigidbody>();
            _originalLayer = model.layer;
            playerParent = this.transform.parent;
        }

        private void Start()
        {
            backInput = Game.Main.GetInputAction("Back", "Global");
            switchInput = Game.Main.GetInputAction("Change Seat", "Player Actions");
        }

        private void OnBackInputPerform(InputAction.CallbackContext context)
        {
            GameObject playerUI = GetPlayerGameObjectFromContext(context);
            // Testing Only
            Player player = playerUI.GetComponent<PlayerReference>().PlayerEntity;
            ExitVehicle(player);
            switchInput.performed -= OnSwitchInputPerform;
            backInput.performed -= OnBackInputPerform;
        }

        private void OnSwitchInputPerform(InputAction.CallbackContext context)
        {
            GameObject playerUI = GetPlayerGameObjectFromContext(context);
            // Testing Only
            Player player = playerUI.GetComponent<PlayerReference>().PlayerEntity;
            ChangeSeat(player);
        }

        private GameObject GetPlayerGameObjectFromContext(InputAction.CallbackContext context)
        {
            // Retrieve the input device from the action context
            var control = context.action.controls.FirstOrDefault();
            if (control != null)
            {
                var device = control.device;
                // Find all PlayerInput components and look for the one associated with this device
                var playerInputs = FindObjectsOfType<PlayerInput>();
                foreach (var playerInput in playerInputs)
                {
                    if (playerInput.devices.Contains(device))
                    {
                        return playerInput.gameObject;
                    }
                }
            }
            return null;
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
            if (SeatsOccupied()) return;

            if (Driver == null)
            {
                EnterDriver(player);
            }
            else
            {
                EnterPassenger(player);
            }

            player.Controls.Rigidbody.isKinematic = true; // "Disable" the Rigidbody
            player.Controls.Rigidbody.useGravity = false;
            foreach (CapsuleCollider collider in player.GetComponents<CapsuleCollider>()) // Disable the colliders
            {
                collider.enabled = false;
            }
            switchInput.performed += OnSwitchInputPerform;
            backInput.performed += OnBackInputPerform; // Subscribe function for exiting
        }

        public bool SeatsOccupied()
        {
            // Check if driver seat has a player set to it
            bool driverOccupied = Driver != null;
            //Check if passenger seat has a player set to it
            bool passengerOccupied = Passengers != null && Passengers.All(passenger => passenger != null);
            return driverOccupied && passengerOccupied;
        }

        public void EnterDriver(Player player)
        {
            Driver = player;
            player.transform.SetParent(Seats[0], false);
            player.transform.localPosition = Vector3.zero;
        }

        public void EnterPassenger(Player player)
        {
            for(int i = 0; i < Passengers.Count; i++)
            {
                SetPassengerSeat(player, i);
                return;
            }
        }

        // Change Seat for Seated Players
        public void ChangeSeat(Player player)
        {
            if (Driver == player)
            {
                // If all passengers are occupied
                if (Passengers != null && Passengers.All(passenger => passenger != null)) return;

                EnterPassenger(player);
                Driver = null;
            }
            else
            {
                if (SeatsOccupied()) return;

                int previousSeatIndex = Passengers.IndexOf(player);
                ChangePassengerSeat(player, previousSeatIndex);
                Passengers[previousSeatIndex] = null;
            }
        }

        // Change Seat for Passenger Players
        public void ChangePassengerSeat(Player player, int previousSeat = 0)
        {
            if (previousSeat == Passengers.Count - 1)
            {
                EnterDriver(player);
            }
            else
            {
                for (int i = previousSeat + 1; i < Passengers.Count + 1; i++)
                {
                    SetPassengerSeat(player, i);
                    return;
                }
            }
        }

        // Set the Seat of the Player to Next Seat Available
        public void SetPassengerSeat(Player player, int seatPosition)
        {
            if (Passengers[seatPosition] == null)
            {
                Passengers[seatPosition] = player;
                player.transform.SetParent(Seats[seatPosition + 1], false);
                player.transform.localPosition = Vector3.zero;
            }
        }

        // Exit Vehicle
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

            if (player == Driver)
            {
                Driver = null;
            }
            else if (Passengers.Contains(player))
            {
                Passengers[Passengers.IndexOf(player)] = null;
            }
        }
    }
}

