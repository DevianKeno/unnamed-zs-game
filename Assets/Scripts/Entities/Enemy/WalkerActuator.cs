using System;

using UnityEngine;
using UnityEngine.AI;

using MEC;


using static UZSG.Entities.EnemyActionStates;

namespace UZSG.Entities
{
    /// Walker, agent actuator part.
    public partial class Walker
    {
        CoroutineHandle _idleRoutine;

        /// <summary>
        /// Initialize the Move & Action states of the Enemy. (i.e. idle, roam, chase, attack, etc.)
        /// </summary>
        void InitializeActuatorEvents()
        {
            MoveStateMachine.AllowReentry = true;
            
            ActionStateMachine[Chase].EnableFixedUpdateCall = true;
            ActionStateMachine[Chase].OnFixedUpdate += OnChaseFixedUpdate;

            ActionStateMachine[Roam].EnableFixedUpdateCall = true;
            ActionStateMachine[Roam].OnFixedUpdate += OnRoamFixedUpdate;

            // ActionStateMachine[Horde].EnableFixedUpdateCall = true;
            // ActionStateMachine[Horde].OnFixedUpdate += OnHordeFixedUpdate;

            ActionStateMachine.OnTransition += OnActionTransition;
        }

        protected virtual void OnActionTransition(StateMachine<EnemyActionStates>.TransitionContext transition)
        {
            switch (transition.From) /// the previous state 
            {
                case Idle:
                {
                    switch (transition.To)
                    {
                        // case Idle:
                        // {
                        //     BeginIdleRoutine();
                        //     break;
                        // }
                        case Roam:
                        {
                            BeginRoamRoutine();
                            break;
                        }
                        case Attack:
                        {
                            /// if player in range prepare attack, else zombie is at idle state
                            if (_hasTargetInAttackRange && !_isAttackOnCooldown && IsAlive)
                            {  
                                ActionAttack();
                            }
                            else if (!_hasTargetInAttackRange && IsAlive)
                            {
                                ActionChase();
                            }
                            break;
                        }
                    }
                    break;
                }
                case Attack:
                {
                    if (transition.To == Idle)
                    {
                        BeginIdleRoutine();
                    }
                    break;
                }
                case Roam:
                {
                    if (transition.To == Idle)
                    {
                        BeginIdleRoutine();
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
                default:
                {
                    break;
                }
            }
        }

        protected virtual void OnRoamFixedUpdate()
        {
            print($"Path status: {NavMeshAgent.pathStatus}");
            print($"Remaining distance: {NavMeshAgent.remainingDistance}");
            /// Check if the enemy has reached its destination and is actively moving, else continue roaming
            if (InRangeOf(NavMeshAgent.destination, 0.1f))
            {
                print($"Agent destination reached");
                ActionStateMachine.ToState(Idle);
            }
            else
            {
                return;
            }
        }

        protected virtual void OnChaseFixedUpdate()
        {
            NavMeshAgent.destination = targetEntity.Position;
        }

        protected virtual void OnHordeFixedUpdate()
        {
            ActionHorde();
        }

        protected virtual void BeginIdleRoutine()
        {
            if (!_hasTargetInSight)
            {
                Timing.KillCoroutines(_idleRoutine);
                _idleRoutine = Timing.RunCoroutine(_IdleRoutine());
            }
        }

        protected virtual void BeginRoamRoutine()
        {
            Timing.KillCoroutines(_idleRoutine);

            print($"Begin roam routine");
            /// Get a random position
            Vector2 randomPoint = UnityEngine.Random.insideUnitCircle.normalized;
            randomPoint *= _roamRadius;
            Vector3 randomPosition = this.Position + new Vector3(randomPoint.x, 0, randomPoint.y);

            print($"Ideal destination set: {randomPosition}");
            if (NavMesh.SamplePosition(randomPosition, out NavMeshHit navHit, _roamRadius, NavMesh.AllAreas))
            {
                print($"Destination set: {navHit.position}");
                NavMeshAgent.destination = navHit.position;
                NavMeshAgent.updatePosition = true;
                NavMeshAgent.updateRotation = true;
                NavMeshAgent.isStopped = false;

                MoveStateMachine.ToState(EnemyMoveStates.Walk);
            }
            else
            {
                print($"Failed to sample NavMesh position!");
            }
            // ActionStateMachine.ToState(Roam); /// I think at it's already at roam state prior to this

            // _roamTimer = UnityEngine.Random.Range(1.0f, _roamInterval); // Reset RoamTime for the next 
            /// Check if the enemy has reached its destination and is actively moving, else continue roaming
            // if (!InRangeOf(NavMeshAgent.destination, 0.1f) && ActionStateMachine.CurrentState.Key != Idle)
            // {
            //     return;
            // }

            // /// Enemy has reached its destination
            // if (NavMeshAgent.hasPath)
            // {
            //     NavMeshAgent.isStopped = true;
            //     NavMeshAgent.updateRotation = false;
            //     /// Clear the path to stop all movement
            //     NavMeshAgent.ResetPath();

            //     MoveStateMachine.ToState(EnemyMoveStates.Idle);
            //     ActionStateMachine.ToState(Idle);
            // }
            // else
            // {
            //     // _roamTimer -= Time.deltaTime;

            //     // if there is a player found chase the player instead of roaming
            //     // if (_hasTargetInSight)
            //     // {
            //     //     BeginChasePhase();
            //     //     return;
            //     // }
            //     // else
            //     // // {
            //     //     if (_roamTimer <= 0)
            //     //     {
            //         // }
            //     // }
            // }
        }

        protected virtual void ActionAttack()
        {
            // if attack not on cd, do animation and set physics to attacking
            if (_isAttackOnCooldown) return;
            
            _isAttacking = true;
            NavMeshAgent.isStopped = true;
            NavMeshAgent.updateRotation = false;
            
            /// TODO: replace to sweep cast
            if (targetEntity is IDamageable damageable)
            {
                damageable.TakeDamage(new DamageInfo(source: this, amount: _attackDamage));
            }
            _isAttacking = false;

            Timing.RunCoroutine(_AttackCooldownTimer());
            /// set the rigid body of the enemy to kinematic
            // Rigidbody.isKinematic = true;
            /// prevent the enemy from moving when in attack range
        }

        protected virtual void ActionChase()
        {
        }

        protected virtual void ActionHorde()
        {
            float distance = 100f;
            SetDestinationBasedOnRotation(distance);
            MoveStateMachine.ToState(EnemyMoveStates.Walk);
        }

        protected void SetDestinationBasedOnRotation(float distance)
        {
            // Calculate the destination by adding the forward direction scaled by the desired distance to the current position
            Vector3 destination = transform.position + hordeTransform.forward * distance;

            // Set the agent's destination to this new point
            NavMeshAgent.SetDestination(destination);
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
    }
}