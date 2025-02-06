using System;
using UZSG.Entities;


namespace UZSG.Players
{
    public enum MoveStates {
        Idle, Jog, Walk, Run, Jump, Crouch, Turn, InVehicle
    }

    public enum StrafeDirection {
        None, Left, Right, Back
    }

    public class MovementStateMachine : StateMachine<MoveStates>
    {        
    }
}
