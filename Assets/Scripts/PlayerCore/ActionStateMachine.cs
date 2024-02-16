using System;
using UZSG.Entities;
using UZSG.Systems;

namespace UZSG.PlayerCore
{        
    public enum ActionStates {
        Idle, Primary, PrimaryHold, Secondary, SecondaryHold, Equip, Dequip
    }

    public class ActionStateMachine : StateMachine<ActionStates>
    {        
    }
}
