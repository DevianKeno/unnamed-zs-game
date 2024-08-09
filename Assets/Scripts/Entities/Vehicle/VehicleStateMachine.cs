using UZSG.Systems;

namespace UZSG.Entities.Vehicles
{
    public enum VehicleStates
    {
        // Not sure yet whether to add 1st to Nth Gear
        Off, Idle, Forward, Reverse, Brake, Stall
    }

    public class VehicleStateChange : StateMachine<VehicleStates>
    {

    }
}
