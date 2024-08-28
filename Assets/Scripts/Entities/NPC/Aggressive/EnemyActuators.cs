using System.Collections;

using UnityEngine;
using UnityEngine.AI;

using UZSG.Systems;

namespace UZSG.Entities
{
    public partial class Enemy : NonPlayerCharacter
    {
        #region Action state handlers

        /// <summary>
        /// Initialize the Move & Action states of the Enemy. (i.e. idle, roam, chase, attack, etc.)
        /// </summary>
        void InitializeActuators()
        {
            actionStateMachine[EnemyActionStates.Idle].OnTransition += OnActionIdleEnter;

            actionStateMachine[EnemyActionStates.Chase].EnableFixedUpdateCall = true;
            actionStateMachine[EnemyActionStates.Chase].OnFixedUpdate += OnChaseFixedUpdate;

            actionStateMachine[EnemyActionStates.Attack].OnTransition += OnAttackEnter;
            actionStateMachine[EnemyActionStates.Roam].OnTransition += OnAttackEnter;
        }

        void OnActionIdleEnter(StateMachine<EnemyActionStates>.TransitionContext e)
        {
            /// Idk what works better
            // if (e.From == EnemyActionStates.Scream)
            if (e.From == EnemyActionStates.Scream && e.To == EnemyActionStates.Idle)
            {
                /// Do something when transitioning from Scream to Idle
            }
        }

        void OnChaseFixedUpdate()
        {
            Chase();
        }

        void OnAttackEnter(StateMachine<EnemyActionStates>.TransitionContext e)
        {
            Attack();
        }
        
        void Idle()
        {

        }

        void Chase()
        {
            /// Set the states to chasing mode
            moveStateMachine.ToState(EnemyMoveStates.Run);

            /// set rigid body to dynamic
            rb.isKinematic = false;

            /// allow enemy movement
            navMeshAgent.isStopped = false;
            navMeshAgent.updateRotation = true;

            /// chase player position
            navMeshAgent.SetDestination(targetEntity.transform.position);

            /// Switch move state machine to run state on chase :)
            ///EnemyMoveStates targetMoveState = EnemyMoveStates.Walk;
            // if (runType == Jog)
            // {
            //     targetMoveState = EnemyMoveStates.Jog;
            // }
            // else if (runType == Run)
            // {
                ///targetMoveState = EnemyMoveStates.Run;
            // }
        }

        IEnumerator FacePlayerAndScream()
        {
            /// Rotate towards the player
            Quaternion targetRotation = Quaternion.LookRotation(targetEntity.Position - transform.position);
            while (Quaternion.Angle(transform.rotation, targetRotation) > 0.1f)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * RotationDamping); /// change 6 to RotationDamping
                yield return null;
            }

            /// Once facing the player, scream
            actionStateMachine.ToState(EnemyActionStates.Scream, lockForSeconds: 2f);
            // _currentActionState = EnemyActionStates.Scream;

            /// Wait for the scream duration
            yield return new WaitForSeconds(2f);

            /// just chase player
            actionStateMachine.ToState(EnemyActionStates.Chase);
        }

        void Roam()
        {
            navMeshAgent.isStopped = false;
            /// Check if the enemy has reached its destination and is actively moving
            if (navMeshAgent.remainingDistance >= 0.002f && navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance)
            {
                navMeshAgent.isStopped = true;
                navMeshAgent.updateRotation = false;
                moveStateMachine.ToState(EnemyMoveStates.Idle);
                actionStateMachine.ToState(EnemyActionStates.Idle);
            }
            else
            {
                /// Continue moving toward the destination
                _roamTime -= Time.deltaTime;
                if (_roamTime <= 0)
                {
                    /// Get a random position
                    _randomDestination = UnityEngine.Random.insideUnitSphere * _roamRadius;
                    _randomDestination += transform.position;

                    NavMesh.SamplePosition(_randomDestination, out NavMeshHit navHit, _roamRadius, NavMesh.AllAreas);

                    /// Set the agent's destination to the random point
                    navMeshAgent.SetDestination(navHit.position);
                    moveStateMachine.ToState(EnemyMoveStates.Walk);
                    actionStateMachine.ToState(EnemyActionStates.Roam);
                    _roamTime = UnityEngine.Random.Range(1.0f, _roamInterval); // Reset RoamTime for the next movement
                    navMeshAgent.updateRotation = true;
                }
            }
        }

        void Attack2() 
        {
            actionStateMachine.ToState(EnemyActionStates.Attack2);
            Debug.Log("Attack2"); 
        }

        void Attack()
        {
            // Set the movement from running to idle
            moveStateMachine.ToState(EnemyMoveStates.Idle);

            /// set the rigid body of the enemy to kinematic
            rb.isKinematic = true;

            /// prevent the enemy from moving when in attack range
            navMeshAgent.isStopped = true;
            navMeshAgent.updateRotation = false;
        }

        void SpecialAttack()
        {
            actionStateMachine.ToState(EnemyActionStates.SpecialAttack);
            Debug.Log("SpecialAttack"); 
        }

        public void SpecialAttack2()
        {
            actionStateMachine.ToState(EnemyActionStates.SpecialAttack2);
            Debug.Log("SpecialAttack2"); 
        }

        void Die()
        {
            actionStateMachine.ToState(EnemyActionStates.Die);
            Game.Tick.OnSecond -= OnSecond;
            Game.Entity.Kill(this);
            Debug.Log("Die");
        }

        void Horde()
        {
            actionStateMachine.ToState(EnemyActionStates.Horde);

            /// Set the starting position and rotation of the zombie
            transform.SetPositionAndRotation(hordeTransform.position, hordeTransform.rotation);

            /// Move forward according to speed
            transform.Translate(Vector3.forward * (_moveSpeed * Time.deltaTime));
        }

        #endregion
    }
}