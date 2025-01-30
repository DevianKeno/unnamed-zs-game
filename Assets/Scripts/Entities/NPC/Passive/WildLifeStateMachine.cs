using System;
using UnityEngine;
using UZSG.Systems;
using UZSG.Players;

namespace UZSG.Entities
{
    public enum WildlifeActionStates {
        Roam, Flee, Die
    }

    /// <summary>
    /// WildlifeActionState[!'s']Machine
    /// </summary>
    public class WildlifeActionStatesMachine : StateMachine<WildlifeActionStates>
    {
        
    }
}