using System;
using UnityEngine;
using UZSG.Systems;
using UZSG.Players;

namespace UZSG.Entities
{
    public enum WildlifeActionStates {
        Roam, RunAway, Die
    }
    public class WildlifeActionStatesMachine : StateMachine<WildlifeActionStates>
    {
        
    }
}