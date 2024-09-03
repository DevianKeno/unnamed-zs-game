using System;
using System.Collections;

using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;
using UZSG.Systems;

using static UZSG.Entities.EnemyActionStates;

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
            actionStateMachine[Chase].EnableFixedUpdateCall = true;
            actionStateMachine[Chase].OnFixedUpdate += OnChaseFixedUpdate;

            actionStateMachine[Roam].EnableFixedUpdateCall = true;
            actionStateMachine[Roam].OnFixedUpdate += OnRoamFixedUpdate;

            actionStateMachine[Horde].EnableFixedUpdateCall = true;
            actionStateMachine[Horde].OnFixedUpdate += OnHordeFixedUpdate;

            actionStateMachine.OnTransition += OnActionTransition;
        }

        void OnActionTransition(StateMachine<EnemyActionStates>.TransitionContext transition)
        {
            switch (transition.From) // the current state 
            {
                case Idle:
                {
                    if (transition.To == Attack)
                    {
                        // if player in range prepare attack, else zombie is at idle state
                        if (_hasTargetInAttackRange && !attackOnCooldown && !IsDead)
                        {  
                            ActionAttack();
                        }
                        else if (!_hasTargetInAttackRange && !IsDead)
                        {
                            ActionChase();
                        }
                    }
                    if (transition.To == Horde)
                    {
                        ActionHorde();
                    }
                    break;
                }
                case Attack:
                {
                    if (transition.To == Idle)
                    {
                        ActionIdle();
                    }
                    break;
                }
                case Roam:
                {
                    if (transition.To == Idle)
                    {
                        ActionIdle();
                    }
                    break;
                }
                case Chase:
                {
                    if (transition.To == Attack)
                    {
                        ActionAttack();
                    }
                    break;
                }
                case Die:
                {
                    ActionDie();
                    break;
                }
                default:
                    break;
            }
        }

        void OnRoamFixedUpdate()
        {
            ActionRoam();
        }

        void OnChaseFixedUpdate()
        {
            ActionChase();
        }

        void OnHordeFixedUpdate()
        {
            ActionHorde();
        }


        IEnumerator FacePlayerAndScream()
        {
            // Face player before screaming
            StartCoroutine(Rotate());

            /// Once facing the player, scream
            actionStateMachine.ToState(Scream, lockForSeconds: 2f);
            // if no target found after screaming
            if (!_hasTargetInSight)
            {
                actionStateMachine.ToState(Roam);
                yield break;
            }
            yield return new WaitForSeconds(2f);

            /// just chase player
            actionStateMachine.ToState(Chase);
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

        IEnumerator IdleThenRoam()
        {
            yield return new WaitForSeconds(4f);
            actionStateMachine.ToState(Roam);
        }

        IEnumerator AttackCounterDownTimer()
        {
            attackOnCooldown = true;
            isAttacking = true;
            float cooldownHolder = attackCooldown;

            while (attackCooldown > 0)
            {
                attackCooldown -= Time.deltaTime; // Reduce cooldown over time
                yield return null;
            }

            // Reset values
            attackOnCooldown = false;
            attackCooldown = cooldownHolder;
            isAttacking = false;
            // if there is a target in range idle, else if not in range chase
            if (_hasTargetInAttackRange)
            {
                actionStateMachine.ToState(Idle);
            }
            else
            {
                actionStateMachine.ToState(Chase);
            }
        }

        void ActionIdle()
        {
            StartCoroutine(IdleThenRoam());
        }

        void ActionRoam()
        {
            navMeshAgent.isStopped = false;
            /// Check if the enemy has reached its destination and is actively moving, else continue roaming
            bool _inPlace = Vector3.Distance(transform.position, _randomDestination) <= distanceThreshold && CurrentActionState != Idle;

            // check if in place and has a path
            if (_inPlace && navMeshAgent.hasPath)
            {
                navMeshAgent.isStopped = true;
                navMeshAgent.updateRotation = false;

                moveStateMachine.ToState(EnemyMoveStates.Idle);
                actionStateMachine.ToState(Idle);

                // Clear the path to stop all movement
                navMeshAgent.ResetPath();
            }
            else
            {
                _roamTime -= Time.deltaTime;
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
                    if (_roamTime <= 0)
                    {
                        /// Get a random position
                        _randomDestination = UnityEngine.Random.insideUnitSphere * _roamRadius;
                        _randomDestination += transform.position;

                        NavMesh.SamplePosition(_randomDestination, out NavMeshHit navHit, _roamRadius, NavMesh.AllAreas);

                        /// Set the agent's destination to the random point
                        _randomDestination = navHit.position;
                        navMeshAgent.SetDestination(navHit.position);
                        moveStateMachine.ToState(EnemyMoveStates.Walk);
                        actionStateMachine.ToState(Roam);
                        _roamTime = UnityEngine.Random.Range(1.0f, _roamInterval); // Reset RoamTime for the next movement
                        navMeshAgent.updateRotation = true;
                    }
                }
            }
        }

        void ActionAttack()
        {
            // if attack not on cd, do animation and set physics to attacking
            if (!attackOnCooldown)
            {
                if (targetEntity.TryGetComponent<IPlayerBeingDamage>(out var damageToPlayer))
                {
                    damageToPlayer.DamagePlayer(attackDamage);
                }
                StartCoroutine(AttackCounterDownTimer());

                /// set the rigid body of the enemy to kinematic
                rb.isKinematic = true;

                /// prevent the enemy from moving when in attack range
                navMeshAgent.isStopped = true;
                navMeshAgent.updateRotation = false;
            }
        }

        void ActionDie()
        {
            // make the enemy ragdoll mode
            IsRagdollOff = false;
            RagdollMode(IsRagdollOff);

            // unsubscribe all state
            actionStateMachine.OnTransition -= OnActionTransition;

            actionStateMachine[Chase].EnableFixedUpdateCall = false;
            actionStateMachine[Chase].OnFixedUpdate -= OnChaseFixedUpdate;

            actionStateMachine[Roam].EnableFixedUpdateCall = false;
            actionStateMachine[Roam].OnFixedUpdate -= OnRoamFixedUpdate;

        }

        void ActionChase()
        {
            /// set rigid body to dynamic   
            rb.isKinematic = false;

            /// allow enemy movement if not in attack range
            navMeshAgent.updateRotation = true;
            navMeshAgent.isStopped = false;

            /// Set the move states to running mode
            moveStateMachine.ToState(EnemyMoveStates.Run);

            /// chase player position
            if (targetEntity != null)
            {
                navMeshAgent.SetDestination(targetEntity.transform.position);
            }
            else
            {
                moveStateMachine.ToState(EnemyMoveStates.Walk);
                actionStateMachine.ToState(Roam);
            }
        }

        void ActionHorde()
        {
            float distance = 100f;
            SetDestinationBasedOnRotation(distance);
            moveStateMachine.ToState(EnemyMoveStates.Walk);
        }

        void SetDestinationBasedOnRotation(float distance)
        {
            // Calculate the destination by adding the forward direction scaled by the desired distance to the current position
            Vector3 destination = transform.position + _hordeTransform.forward * distance;

            // Set the agent's destination to this new point
            navMeshAgent.SetDestination(destination);
            Debug.Log("is moving forward");
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

        protected override void OnKill()
        {
            Game.Tick.OnSecond -= OnSecond;
            Debug.Log("Die");
        }

        void Die()
        {
            actionStateMachine.ToState(EnemyActionStates.Die);
            Kill(this);
        }*/

        #endregion
    }
}