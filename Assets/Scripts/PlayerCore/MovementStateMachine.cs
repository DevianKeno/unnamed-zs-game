using System;
using UZSG.Entities;
using UZSG.Systems;

namespace UZSG.Players
{
    public enum MoveStates {
        Jog, Idle, Walk, Run, Jump, Crouch, Turn
    }

    public enum StrafeDirection {
        None, Left, Right, Back
    }

    public class MovementStateMachine : StateMachine<MoveStates>
    {        
    }
}
