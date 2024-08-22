using System;
using UnityEngine;
using UZSG.Systems;
using UZSG.Players;

namespace UZSG.Entities
{
    public enum EnemyMoveStates {
        Idle, Walk, Run, Crawl,
    }
    
    public class EnemyMoveStateMachine : StateMachine<EnemyMoveStates>
    {

    }
}