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
    public class VehicleEntity : Entity, IInteractable
    {
        [Header("Vehicle Information")]
        [SerializeField] VehicleData vehicle; // vehicle data
        [SerializeField] VehicleController _vehicleController;


        #region Vehicle Seats
        [Header("Vehicle Seats")]
        public Player Driver; // driver of the vehicle
        public List<Player> Passengers; // passenger of the vehicle
        public List<Transform> Seats; // transform of the seats of the vehicle
        #endregion

        #region Other Important Values
        Transform _playerParent; // refers to the parent of the player in the game world
        #endregion

        [Header("Vehicle Parts")]
        public GameObject Model;
        public List<WheelCollider> FrontVehicleWheels;
        public List<WheelCollider> RearVehicleWheels;

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
            Model = transform.Find("Vehicle Body").gameObject;
            _originalLayer = Model.layer;
            _playerParent = this.transform.parent;
        }

        public void OnLookEnter()
        {
            if (Model != null && Driver == null)
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

            EnterVehicle(player);
        }

        public void EnterVehicle(Player player)
        {
            if (SeatsOccupied()) return;

            player.Controls.SetControl("Jump", false);

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
            _vehicleController.EnableGeneralVehicleControls();
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
            _vehicleController.EnableVehicleControls();
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
                _vehicleController.DisableVehicleControls();
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

            player.transform.SetParent(_playerParent, false); // Set the player position to the parent

            player.Controls.Rigidbody.position = new(Model.transform.position.x + 2,
                                                     Model.transform.position.y,
                                                     Model.transform.position.z);

            player.Controls.Rigidbody.rotation = Quaternion.identity;

            if (player == Driver)
            {
                Driver = null;
                _vehicleController.DisableVehicleControls();
            }
            else if (Passengers.Contains(player))
            {
                Passengers[Passengers.IndexOf(player)] = null;
            }
            _vehicleController.DisableGeneralVehicleControls();
            player.Controls.SetControl("Jump", true);
        }
    }
}

