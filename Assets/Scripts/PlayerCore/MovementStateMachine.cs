using System;
using UZSG.Entities;
using UZSG.Systems;

namespace UZSG.PlayerCore
{
    public enum MoveStates {
        Idle, Walk, Run, Jump, Crouch, CrouchWalk,
    }

    public class MovementStateMachine : StateMachine<MoveStates>
    {        
    }
}
