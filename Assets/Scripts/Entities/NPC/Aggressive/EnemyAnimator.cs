using UnityEngine;
using UnityEngine.AI;

using UZSG.Data;
using UZSG.Systems;
using UZSG.Interactions;
using UZSG.Attributes;
using System;

namespace UZSG.Entities
{
    public partial class Enemy : NonPlayerCharacter, IPlayerDetectable
    {
        void InitializeAnimator()
        {
            moveStateMachine.OnStateChanged += OnMoveStateChanged;
            actionStateMachine.OnStateChanged += OnActionStateChanged;
        }

        void OnMoveStateChanged(object sender, StateMachine<EnemyMoveStates>.StateChangedContext e)
        {
            if (e.To == EnemyMoveStates.Idle)
            {
                animator.CrossFade("idle", 0.1f);
            }
            else if (e.To == EnemyMoveStates.Run)
            {
                animator.CrossFade("jog_forward", 0.1f);
            }
        }

        void OnActionStateChanged(object sender, StateMachine<EnemyActionStates>.StateChangedContext e)
        {
            if (e.To == EnemyActionStates.Idle)
            {
                animator.CrossFade("idle", 0.1f);
            }
            else if (e.To == EnemyActionStates.Roam)
            {
                animator.CrossFade("walk_mutant", 0.1f);
            }
            else if (e.To == EnemyActionStates.Scream)
            {
                animator.CrossFade("scream", 0.1f);
            }
        }
    }
}