using System;
using UnityEngine;
using UZSG.Systems;
using UZSG.Players;

namespace UZSG.Entities
{
    public enum WildLifeActionStates {
        Roam, RunAway, Die
    }
    public class WildLifeActionStatesMachine : StateMachine<WildLifeActionStates>
    {
        
    }
}