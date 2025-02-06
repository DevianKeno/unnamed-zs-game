using System;
using UnityEngine;

using UZSG.Players;

namespace UZSG.Entities
{
    public enum EnemyActionStates {
        Idle, Roam, Scream, Chase, Attack, Attack2, SpecialAttack, SpecialAttack2, Die
    }
    public class EnemyActionStateMachine : StateMachine<EnemyActionStates>
    {

    }
}