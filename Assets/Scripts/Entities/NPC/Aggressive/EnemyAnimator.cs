using System.Collections;

using UnityEngine;

using UZSG.Systems;
using UZSG.Interactions;

namespace UZSG.Entities
{
    public partial class Enemy : NonPlayerCharacter, IPlayerDetectable
    {
        void InitializeAnimator()
        {
            moveStateMachine.OnTransition += OnMoveStateChanged;
            actionStateMachine.OnTransition += OnActionStateChanged;
        }

        /// <summary>
        /// This method changes the state of the enemy, and runs in two options:
        /// - continuous, animation keeps playing infinitely unless changed.
        /// - discrete, animation plays certain times and stops.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void OnMoveStateChanged(StateMachine<EnemyMoveStates>.TransitionContext e)
        {
            if (e.To == EnemyMoveStates.Idle)
            {
                animator.CrossFade("idle_drunk", 0.1f);
            }
            else if (e.To == EnemyMoveStates.Run)
            {
                animator.CrossFade("jog_forward", 0.1f);
            }
            else if (e.To == EnemyMoveStates.Walk)
            {
                animator.CrossFade("walk_mutant", 0.1f);
            }
        }

        void OnActionStateChanged(StateMachine<EnemyActionStates>.TransitionContext e)
        {
            if (e.To == EnemyActionStates.Idle)
            {
                animator.CrossFade("idle_drunk", 0.1f);
            }
            else if (e.To == EnemyActionStates.Scream)
            {
                animator.CrossFade("scream", 0.1f);
            }
        }
    }
}