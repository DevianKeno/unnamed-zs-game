using UnityEngine;

namespace UZSG.Entities
{
    /// Walker, Animator part.
    public partial class Walker
    {
        /// <summary>
        /// Start listening to state machine transitions and play the appropriate animations.
        /// </summary>
        void InitializeAnimatorEvents()
        {
            MoveStateMachine.OnTransition += AnimateMoveStateChanged;
            ActionStateMachine.OnTransition += AnimateActionStateChanged;
        }

        /// <summary>
        /// This method changes the state of the enemy, and runs in two options:
        /// - continuous, animation keeps playing infinitely unless changed.
        /// - discrete, animation plays certain times and stops.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void AnimateMoveStateChanged(StateMachine<EnemyMoveStates>.TransitionContext e)
        {
            if (e.To == EnemyMoveStates.Idle)
            {
                Animator.CrossFade("idle", 0.1f);
            }
            else if (e.To == EnemyMoveStates.Run)
            {
                Animator.CrossFade("jog_drunk", 0.1f);
            }
            else if (e.To == EnemyMoveStates.Walk)
            {
                Animator.CrossFade("walk_mutant", 0.1f);
            }
        }

        void AnimateActionStateChanged(StateMachine<EnemyActionStates>.TransitionContext e)
        {
            if (e.To == EnemyActionStates.Idle)
            {
                Animator.CrossFade("idle", 0.1f);
            }
            else if (e.To == EnemyActionStates.Scream)
            {
                Animator.CrossFade("scream", 0.1f);
            }
            else if (e.To == EnemyActionStates.Attack)
            {
                Animator.CrossFade("attack", 0.1f);
            }
        }
    }
}