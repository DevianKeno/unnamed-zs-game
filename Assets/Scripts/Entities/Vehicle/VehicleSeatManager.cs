using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UZSG.Entities.Vehicles
{
    public class VehicleSeatManager : MonoBehaviour
    {
        VehicleEntity _vehicle;
        VehicleController _vehicleController;
        VehicleAudioManager _audioManager;

        #region Vehicle Seats
        [Header("Vehicle Player Seats")]
        public Player Driver; // driver of the vehicle
        public List<Player> Passengers; // passenger of the vehicle

        [Header("Vehicle Camera Transforms")]
        public List<Transform> FPPCameraView; // transform of the seats of the vehicle
        public Transform TPPCameraView;
        #endregion

        #region Other Important Values
        Transform _playerParent; // refers to the parent of the player in the game world
        Transform _mainCameraParent; // refers to the Main Camera Parent
        #endregion

        private void Awake()
        {
            _vehicle = gameObject.GetComponent<VehicleEntity>();
            _vehicleController = gameObject.GetComponent<VehicleController>();
            _audioManager = gameObject.GetComponent<VehicleAudioManager>();
            _playerParent = this.transform.parent;
        }

        public void EnterVehicle(Player player)
        {
            bool[] _areSeatsOccupied = SeatsOccupied();
            if (_areSeatsOccupied[0] && _areSeatsOccupied[1]) return;

            if (Driver == null)
            {
                EnterDriver(player);
            }
            else
            {
                EnterPassenger(player);
            }
            _mainCameraParent = player.MainCamera.transform.parent;
            player.MainCamera.transform.SetParent(TPPCameraView, false);
            player.Model.rotation = Quaternion.LookRotation(_vehicle.Model.transform.forward);
            player.Controls.Rigidbody.isKinematic = true; // "Disable" the Rigidbody
            player.Controls.Rigidbody.useGravity = false;
            foreach (CapsuleCollider collider in player.GetComponents<CapsuleCollider>()) // Disable the colliders
            {
                collider.enabled = false;
            }
            _vehicleController.EnableGeneralVehicleControls(player);
        }

        public bool[] SeatsOccupied()
        {
            // Check if driver seat has a player set to it
            bool driverOccupied = Driver != null;
            //Check if passenger seat has a player set to it
            bool passengerOccupied = Passengers != null && Passengers.All(passenger => passenger != null);
            return new[] { driverOccupied, passengerOccupied };
        }

        public void EnterDriver(Player player)
        {
            Driver = player;
            player.transform.SetParent(FPPCameraView[0], false);
            player.transform.localPosition = Vector3.zero;
            _vehicleController.EnableVehicleControls();
        }

        public void EnterPassenger(Player player)
        {
            for (int i = 0; i < Passengers.Count; i++)
            {
                SetPassengerSeat(player, i);
                return;
            }
        }

        // Set the Seat of the Player to Next Seat Available
        public void SetPassengerSeat(Player player, int seatPosition)
        {
            if (Passengers[seatPosition] == null)
            {
                Passengers[seatPosition] = player;
                player.transform.SetParent(FPPCameraView[seatPosition + 1], false);
                player.transform.localPosition = Vector3.zero;
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
                bool[] _areSeatsOccupied = SeatsOccupied();
                if (_areSeatsOccupied[0] && _areSeatsOccupied[1]) return;

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

        public void ChangeVehicleView(Player player)
        {
            if (_mainCameraParent.transform.Find("Main Camera") == null)
            {
                player.MainCamera.transform.SetParent(_mainCameraParent, false);
            }
            else
            {
                player.MainCamera.transform.SetParent(TPPCameraView, false);
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

            player.MainCamera.transform.SetParent(_mainCameraParent, false);

            _mainCameraParent = null;

            player.Controls.Rigidbody.position = new(transform.position.x + 2,
                                                     transform.position.y,
                                                     transform.position.z);

            player.Controls.Rigidbody.rotation = Quaternion.identity;
            player.Model.transform.rotation = Quaternion.identity;

            if (player == Driver)
            {
                Driver = null;
                _vehicleController.DisableVehicleControls();
            }
            else if (Passengers.Contains(player))
            {
                Passengers[Passengers.IndexOf(player)] = null;
            }
            _vehicleController.DisableGeneralVehicleControls(player);
            CheckPassengers();
        }

        private void CheckPassengers()
        {
            bool[] _areSeatsOccupied = SeatsOccupied();
            if ((_areSeatsOccupied[0] && _areSeatsOccupied[1]) == false)
            {
                print("0");
                _audioManager.NoPlayerInVehicle();
                _audioManager.NoPlayerInVehicle();
            }
            else if ((_areSeatsOccupied[0] || _areSeatsOccupied[1]) == false)
            {
                print("1");
                _vehicleController.DisableVehicle();
                
            }
        }
    }
}