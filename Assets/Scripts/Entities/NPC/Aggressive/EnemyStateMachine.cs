using System;
using UnityEngine;
using UZSG.Systems;
using UZSG.Players;

namespace UZSG.Entities
{
    public enum EnemyActionStates {
        Chase, Attack, Roam, Attack2, SpecialAttack, SpecialAttack2, Die
    }
    public abstract class EnemyActionStatesMachine : StateMachine<EnemyActionStates>
    {
        public abstract bool _isInSiteRange(); // determines if enemy can chase player or roam map
        public abstract bool _isInAttackrange(); // determines if enemy can attack player
        public abstract bool _isNoHealth(); // determines if the enemy is dead
        public abstract bool _isSpecialAttackTriggered(); // determines if an event happened that triggered special attack 1
        public abstract bool _isSpecialAttackTriggered2(); // determines if an event happened that triggered special attack 2
        public abstract void executeAction(); // given the right condition execute the action
    }
}