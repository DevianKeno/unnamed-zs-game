using System;
using UnityEngine;

using UZSG.Players;

namespace UZSG.Entities
{
    public enum WildlifeActionStates {
        Roam, Flee, Die
    }

    /// <summary>
    /// WildlifeActionStateMachine
    /// </summary>
    public class WildlifeActionStateMachine : StateMachine<WildlifeActionStates>
    {
        
    }
}