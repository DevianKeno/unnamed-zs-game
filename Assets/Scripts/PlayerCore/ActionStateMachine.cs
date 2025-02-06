using System;

using UZSG.Entities;


namespace UZSG.Players
{        
    public enum ActionStates {
        Idle, Primary, PrimaryHold, PrimaryRelease, Secondary, SecondaryHold, SecondaryRelease, Equip, Dequip
    }

    public class ActionStateMachine : StateMachine<ActionStates>
    {
        
    }
}
