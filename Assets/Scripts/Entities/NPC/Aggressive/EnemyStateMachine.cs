using System;
using UnityEngine;
using UZSG.Systems;
using UZSG.Players;

namespace UZSG.Entities
{
    public enum EnemyActionStates {
        Chase, Attack, Roam, Attack2, SpecialAttack, SpecialAttack2, Die
    }
    public class EnemyActionStatesMachine : StateMachine<EnemyActionStates>
    {

    }
}