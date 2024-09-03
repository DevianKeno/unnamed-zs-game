using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UZSG.FPP;

namespace UZSG.Entities.Vehicles
{
    public class VehicleSeatManager : MonoBehaviour
    {
        public VehicleEntity Vehicle { get; private set; }


        #region Vehicle Seats

        [Header("Vehicle Occupants")]
        public Player Driver; // driver of the vehicle
        public List<Player> Passengers; // passenger of the vehicle

        [Header("Vehicle Seat Transform")]
        public List<Transform> VehicleSeats; // transform of the seats of the vehicle

        #endregion


        #region Other Important Values

        RigidbodyConstraints _originalPlayerRbConstraints;
        Transform _playerParent; // refers to the parent of the player in the game world
        
        #endregion


        private void Awake()
        {
            Vehicle = GetComponent<VehicleEntity>();
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

        public void EnterAsDriver(Player player)
        {
            // Set Driver to Player
            Driver = player;
            player.Actions.EnterVehicle(Vehicle);
            player.transform.SetParent(VehicleSeats[0], worldPositionStays: false);
            player.transform.localPosition = Vector3.zero;

            Vehicle.InputHandler.ToggleVehicleControls(true);
        }

        public void EnterAsPassenger(Player player)
        {
            for (int i = 0; i < Passengers.Count; i++)
            {
                EnterAsPassenger(player, i);
                return;
            }
        }

        public enum SeatPosition {
            Driver, Shotgun, /* RearLeft, RearRight */
        }
        
        public void SetPassengerSeat(Player player, SeatPosition position)
        {
        }

        public class VehicleSeat
        {
            public Entity Entity;
            public SeatPosition Position;
            public bool IsOccupied => Entity != null;
            
            Transform transform;
        }

        public void EnterAsPassenger(Player player, int seatPosition)
        {
            if (Passengers[seatPosition] == null)
            {
                Passengers[seatPosition] = player;
                player.Actions.EnterVehicle(Vehicle);
                player.transform.SetParent(VehicleSeats[seatPosition + 1], worldPositionStays: false);
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
            /// if (?? && !HasDriver)
            if (previousSeat == Passengers.Count - 1 && Driver == null)
            {
                EnterAsDriver(player);
            }
            else
            {
                for (int i = previousSeat + 1; i < Passengers.Count + 1; i++)
                {
                    EnterAsPassenger(player, i);
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

                EnterAsPassenger(player);
                Driver = null;
                Vehicle.InputHandler.ToggleVehicleControls(false);
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

            _playerParent = player.transform.parent;

            if (Driver == null) /// bool HasDriver
            {
                EnterAsDriver(player);
            }
            else
            {
                EnterAsPassenger(player);
            }

            // Toggles Camera View to TPP
            Vehicle.CameraManager.ToggleCamera(player, "TPP");

            // Toggle Third Person Model to Match Vehicle Transform
            player.Model.rotation = Quaternion.LookRotation(Vehicle.Model.transform.forward);

            // Toggle Player Physics
            TogglePlayerPhysics(player, false);

            // Toggle Vehicle Controls
            Vehicle.InputHandler.ToggleGeneralControls(player, true);
        }

        public void ExitVehicle(Player player)
        {
            TogglePlayerPhysics(player, true);

            Vehicle.CameraManager.ToggleCamera(player, "FPP");

            // player.Actions.ExitVehicle(Vehicle);
            player.transform.SetParent(_playerParent, false); // Set the player position to the environment

            player.Controls.Rigidbody.position = new(transform.position.x + 2,
                                                     transform.position.y,
                                                     transform.position.z + 2);

            player.Controls.Rigidbody.rotation = Quaternion.identity;
            player.Model.transform.rotation = Quaternion.identity;

            if (player == Driver)
            {
                Driver = null;
                Vehicle.InputHandler.ToggleVehicleControls(false);
            }
            else if (Passengers.Contains(player))
            {
                Passengers[Passengers.IndexOf(player)] = null;
            }

            // Disable Vehicle Controls
            Vehicle.InputHandler.ToggleGeneralControls(player, false);
            CheckPassengers();
        }

        private void CheckPassengers()
        {
            bool[] _areSeatsOccupied = SeatsOccupied();
            if ((_areSeatsOccupied[0] && _areSeatsOccupied[1]) == false)
            {
                print("0");
                Vehicle.AudioManager.NoPlayerInVehicle();
                Vehicle.AudioManager.NoPlayerInVehicle();
            }
            else if ((_areSeatsOccupied[0] || _areSeatsOccupied[1]) == false)
            {
                print("1");
                Vehicle.Controller.DisableVehicle();
            }
        }
    }
}