using System;
using UnityEngine;
using UZSG.Systems;
using UZSG.Players;

namespace UZSG.Entities
{
    public enum EnemyActionStates {
        Idle, Roam, Scream, Chase, Attack, Attack2, SpecialAttack, SpecialAttack2, Die, Horde
    }
    public class EnemyActionStateMachine : StateMachine<EnemyActionStates>
    {

    }
}