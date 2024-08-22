using UnityEngine;
using UnityEngine.AI;

using UZSG.Data;
using UZSG.Systems;
using UZSG.Interactions;
using UZSG.Attributes;
using System.Collections.Generic;
using System;
using UZSG.Players;

using static UZSG.Entities.EnemyActionStates;

namespace UZSG.Entities
{
    public partial class Enemy : NonPlayerCharacter, IPlayerDetectable
    {
        #region Agent actuator

        /// <summary>
        /// Initialize the Move & Action states of the Enemy. (i.e. idle, roam, chase, attack, etc.)
        /// </summary>
        void InitializeActuators() /// uncalled yet
        {
            actionStateMachine[EnemyActionStates.Idle].OnEnter += OnActionIdleEnter;
            actionStateMachine[EnemyActionStates.Chase].OnEnter += OnActionChaseEnter;
            actionStateMachine[EnemyActionStates.Chase].OnEnter += OnActionChaseEnter;
            actionStateMachine[EnemyActionStates.Chase].OnEnter += OnActionChaseEnter;
        }

        void OnActionIdleEnter(object sender, State<EnemyActionStates>.ChangedContext e)
        {
            if (e.PreviousState == EnemyActionStates.Scream)
            {

            }
        }

        void OnActionChaseEnter(object sender, State<EnemyActionStates>.ChangedContext e)
        {
            /// might run twice because of this and ExecuteAction()
            // Chase();
        }

        /// <summary>
        /// Execute an action depending on what state the entity is on
        /// </summary>
        public void ExecuteAction(EnemyActionStates state) 
        {
            switch (state)
            {
                case EnemyActionStates.Idle:
                    Idle();
                    break;

                case EnemyActionStates.Chase:
                    Chase();
                    break;

                case EnemyActionStates.Roam:
                    Roam();
                    break;

                case EnemyActionStates.Attack2:
                    Attack2();
                    break;

                case EnemyActionStates.Attack:
                    Attack();
                    break;

                case EnemyActionStates.Die:
                    Die();
                    break;

                case EnemyActionStates.SpecialAttack:
                    SpecialAttack();
                    break;

                case EnemyActionStates.SpecialAttack2:
                    SpecialAttack2();
                    break;

                case EnemyActionStates.Horde:
                    Horde();
                    break;
            }
        }

        void Idle()
        {

        }

        void Chase()
        {
            actionStateMachine.ToState(EnemyActionStates.Chase);

            /// set rigid body to dynamic
            rb.isKinematic = false;

            /// allow enemy movement
            navMeshAgent.isStopped = false;
            navMeshAgent.updateRotation = true;

            /// chase player position
            navMeshAgent.SetDestination(targetEntity.transform.position);

            /// Switch move state machine to run state on chase :)
            EnemyMoveStates targetMoveState = EnemyMoveStates.Walk;
            // if (runType == Jog)
            // {
            //     targetMoveState = EnemyMoveStates.Jog;
            // }
            // else if (runType == Run)
            // {
                targetMoveState = EnemyMoveStates.Run;
            // }
            
            moveStateMachine.ToState(targetMoveState);
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
            actionStateMachine.ToState(EnemyActionStates.Attack);

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