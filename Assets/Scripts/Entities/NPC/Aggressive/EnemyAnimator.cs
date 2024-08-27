using UnityEngine;
using UnityEngine.AI;

using UZSG.Data;
using UZSG.Systems;
using UZSG.Interactions;
using UZSG.Attributes;
using System;
using UZSG.Players;
using System.Collections;

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
                animator.CrossFade("idle", 0.1f);
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
                animator.CrossFade("idle", 0.1f);
            }
            else if (e.To == EnemyActionStates.Scream)
            {
                animator.CrossFade("scream", 0.1f);
            }
        }

        void SwitchAndLoopAnimation(string animationName)
        {
            // CrossFade to transition to the desired animation
            animator.CrossFade(animationName, 0.1f);

            // Use a coroutine to wait until the crossfade transition is done
            StartCoroutine(LockAnimation(animationName));
        }

        IEnumerator LockAnimation(string animationName)
        {
            // Wait until the crossfade is done
            yield return new WaitForSeconds(0.1f);

            // Lock the animation in a loop
            animator.Play(animationName);
        }
    }
}