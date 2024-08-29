using System;
using System.Collections;

using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;
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
            actionStateMachine[EnemyActionStates.Chase].EnableFixedUpdateCall = true;
            actionStateMachine[EnemyActionStates.Chase].OnFixedUpdate += OnChaseFixedUpdate;

            actionStateMachine[EnemyActionStates.Roam].EnableFixedUpdateCall = true;
            actionStateMachine[EnemyActionStates.Roam].OnFixedUpdate += OnRoamFixedUpdate;

            actionStateMachine[EnemyActionStates.Attack].OnTransition += OnAttackEnter;
            actionStateMachine[EnemyActionStates.Die].OnTransition += OnDieEnter;
            actionStateMachine[EnemyActionStates.Idle].OnTransition += OnIdleEnter;
        }

        private void OnDieEnter(StateMachine<EnemyActionStates>.TransitionContext context)
        {
            Die();
        }

        void OnIdleEnter(StateMachine<EnemyActionStates>.TransitionContext e)
        {
            if (_hasTargetInAttackRange)
            {
                
            }
            // if enemy went from screaming to idling
            else if (e.From == EnemyActionStates.Scream && e.To == EnemyActionStates.Idle)
            {
                /// roam, idle, scream
            }
        }

        void OnRoamFixedUpdate()
        {
            Roam();
        }

        void OnChaseFixedUpdate()
        {
            Chase();
        }

        void OnAttackEnter(StateMachine<EnemyActionStates>.TransitionContext e)
        {
            Attack();
        }


        IEnumerator FacePlayerAndScream()
        {
            // Face player before screaming
            StartCoroutine(Rotate());

            /// Once facing the player, scream
            actionStateMachine.ToState(EnemyActionStates.Scream, lockForSeconds: 2f);
            yield return new WaitForSeconds(2f);

            /// just chase player
            actionStateMachine.ToState(EnemyActionStates.Chase);
        }

        IEnumerator Rotate()
        {
            isAlreadyRotating = true;
            /// Rotate towards the player
            Quaternion targetRotation = Quaternion.LookRotation(targetEntity.Position - transform.position);
            while (Quaternion.Angle(transform.rotation, targetRotation) > rotationThreshold)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * RotationDamping);
                yield return null;
            }

            isAlreadyRotating = false;
        }

        void Roam()
        {
            navMeshAgent.isStopped = false;
            /// Check if the enemy has reached its destination and is actively moving
            if (navMeshAgent.remainingDistance >= 1f && navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance)
            {
                navMeshAgent.isStopped = true;
                navMeshAgent.updateRotation = false;
                moveStateMachine.ToState(EnemyMoveStates.Idle);
                actionStateMachine.ToState(EnemyActionStates.Idle);
            }
            else
            {
                // if there is a player found chase the player instead of roaming
                if (_hasTargetInSight)
                {
                    // Scream at player then chase
                    if (!_hasAlreadyScreamed)
                    {
                        _hasAlreadyScreamed = true;
                        StartCoroutine(FacePlayerAndScream());
                    }
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

        void Die()
        {
            Game.Tick.OnSecond -= OnSecond;
            Game.Entity.Kill(this);
            Debug.Log("Die");
        }

        void Chase()
        {
            Debug.Log("Is Chasing");
            /// set rigid body to dynamic
            rb.isKinematic = false;

            /// allow enemy movement if not in attack range
            navMeshAgent.updateRotation = true;
            navMeshAgent.isStopped = false;

            /// Set the move states to running mode
            moveStateMachine.ToState(EnemyMoveStates.Run);

            /// chase player position
            navMeshAgent.SetDestination(targetEntity.transform.position);
        }

        /*void Attack2() 
        {
            actionStateMachine.ToState(EnemyActionStates.Attack2);
            Debug.Log("Attack2"); 
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

        void Horde()
        {
            actionStateMachine.ToState(EnemyActionStates.Horde);

            /// Set the starting position and rotation of the zombie
            transform.SetPositionAndRotation(hordeTransform.position, hordeTransform.rotation);

            /// Move forward according to speed
            transform.Translate(Vector3.forward * (_moveSpeed * Time.deltaTime));
        }*/

        #endregion
    }
}