using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UZSG.FPP;

namespace UZSG.Entities.Vehicles
{
    public class VehicleSeatManager : MonoBehaviour
    {
        VehicleEntity _vehicle;

        #region Vehicle Seats
        [Header("Vehicle Occupants")]
        public Player Driver; // driver of the vehicle
        public List<Player> Passengers; // passenger of the vehicle

        [Header("Vehicle Seat Transform")]
        public List<Transform> VehicleSeats; // transform of the seats of the vehicle
        #endregion

        #region Other Important Values
        Transform _playerParent; // refers to the parent of the player in the game world
        #endregion

        private void Awake()
        {
            _vehicle = gameObject.GetComponent<VehicleEntity>();
            _playerParent = this.transform.parent;
        }

        public bool[] SeatsOccupied()
        {
            // Check if driver seat has a player set to it
            bool driverOccupied = Driver != null;

            // Check if passenger seat has a player set to it
            bool passengerOccupied = Passengers != null && Passengers.All(passenger => passenger != null);
            return new[] { driverOccupied, passengerOccupied };
        }

        public void TogglePlayerPhysics(Player player, bool isEnabled)
        {
            // Toggle Player Variables
            player.Controls.Rigidbody.isKinematic = !isEnabled; // "Disable" the Rigidbody
            player.Controls.Rigidbody.useGravity = isEnabled;
            foreach (CapsuleCollider collider in player.GetComponents<CapsuleCollider>()) // Disable the colliders
            {
                collider.enabled = isEnabled;
            }
        }

        public void EnterDriver(Player player)
        {
            // Set Driver to Player
            Driver = player;
            player.transform.SetParent(VehicleSeats[0], false);
            player.transform.localPosition = Vector3.zero;
            _vehicle.InputHandler.ToggleVehicleControls(true);
        }

        public void EnterPassenger(Player player)
        {
            for (int i = 0; i < Passengers.Count; i++)
            {
                SetPassengerSeat(player, i);
                return;
            }
        }

        public void SetPassengerSeat(Player player, int seatPosition)
        {
            if (Passengers[seatPosition] == null)
            {
                Passengers[seatPosition] = player;
                player.transform.SetParent(VehicleSeats[seatPosition + 1], false);
                player.transform.localPosition = Vector3.zero;
            }
        }

        /// <summary>
        /// Set player to Driver seat or an unoccupied passenger seat
        /// </summary>
        /// <param name="player"></param>
        /// <param name="previousSeat"></param>
        public void ChangePassengerSeat(Player player, int previousSeat = 0)
        {
            if (previousSeat == Passengers.Count - 1 && Driver == null)
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

        public void ChangeSeat(Player player)
        {
            if (Driver == player)
            {
                // If all passengers are occupied
                if (Passengers != null && Passengers.All(passenger => passenger != null)) return;

                EnterPassenger(player);
                Driver = null;
                _vehicle.InputHandler.ToggleVehicleControls(false);
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

            // Toggles Camera View to TPP
            _vehicle.CameraManager.ToggleCamera(player, "TPP");

            // Toggle Third Person Model to Match Vehicle Transform
            player.Model.rotation = Quaternion.LookRotation(_vehicle.Model.transform.forward);

            // Toggle Player Physics
            TogglePlayerPhysics(player, false);

            // Toggle Vehicle Controls
            _vehicle.InputHandler.ToggleGeneralControls(player, true);
        }

        public void ExitVehicle(Player player)
        {
            TogglePlayerPhysics(player, true);

            _vehicle.CameraManager.ToggleCamera(player, "FPP");

            player.transform.SetParent(_playerParent, false); // Set the player position to the environment

            player.Controls.Rigidbody.position = new(transform.position.x + 2,
                                                     transform.position.y,
                                                     transform.position.z + 2);

            player.Controls.Rigidbody.rotation = Quaternion.identity;
            player.Model.transform.rotation = Quaternion.identity;

            if (player == Driver)
            {
                Driver = null;
                _vehicle.InputHandler.ToggleVehicleControls(false);
            }
            else if (Passengers.Contains(player))
            {
                Passengers[Passengers.IndexOf(player)] = null;
            }

            // Disable Vehicle Controls
            _vehicle.InputHandler.ToggleGeneralControls(player, false);
            CheckPassengers();
        }

        private void CheckPassengers()
        {
            bool[] _areSeatsOccupied = SeatsOccupied();
            if ((_areSeatsOccupied[0] && _areSeatsOccupied[1]) == false)
            {
                print("0");
                _vehicle.AudioManager.NoPlayerInVehicle();
                _vehicle.AudioManager.NoPlayerInVehicle();
            }
            else if ((_areSeatsOccupied[0] || _areSeatsOccupied[1]) == false)
            {
                print("1");
                _vehicle.Controller.DisableVehicle();
            }
        }
    }
}