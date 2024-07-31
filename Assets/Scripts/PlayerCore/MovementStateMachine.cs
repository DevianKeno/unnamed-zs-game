using System;
using UZSG.Entities;
using UZSG.Systems;

namespace UZSG.Players
{
    public enum MoveStates {
        Idle, Walk, Run, Jump, Crouch, CrouchWalk,
    }

    public enum StrafeDirection {
        None, Left, Right, Back
    }

    public class MovementStateMachine : StateMachine<MoveStates>
    {        
    }
}
