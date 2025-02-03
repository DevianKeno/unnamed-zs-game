using UnityEngine;
using UnityEngine.AI;
using UZSG.Systems;

using static UZSG.Entities.EnemyActionStates;

namespace UZSG.Entities
{
    public partial class Enemy : NonPlayerCharacter, IDamageSource
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
                        if (_hasTargetInAttackRange && !_isAttackOnCooldown && !IsDead)
                        {  
                            ActionAttack();
                        }
                        else if (!_hasTargetInAttackRange && !IsDead)
                        {
                            ActionChase();
                        }
                    }
                    if (transition.To == Die)
                    {
                        ActionDie();
                    }
                    if (transition.To == Horde)
                    {
                        ActionHorde();
                    }
                    break;
                }
                case Attack:
                {
                    if (transition.To == Die)
                    {
                        ActionDie();
                    }
                    if (transition.To == Idle)
                    {
                        ActionIdle();
                    }
                    break;
                }
                case Roam:
                {
                    if (transition.To == Die)
                    {
                        ActionDie();
                    }
                    if (transition.To == Idle)
                    {
                        ActionIdle();
                    }
                    break;
                }
                case Chase:
                {
                    if (transition.To == Die)
                    {
                        ActionDie();
                    }
                    if (transition.To == Attack)
                    {
                        ActionAttack();
                    }
                    break;
                }
                case Horde:
                {
                    if (transition.To == Die)
                    {
                        ActionDie();
                    }
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

        void ActionIdle()
        {
            if (!_hasTargetInSight)
            {
                StartCoroutine(IdleThenRoam());
            }
        }

        void ActionRoam()
        {
            navMeshAgent.isStopped = false;
            /// Check if the enemy has reached its destination and is actively moving, else continue roaming
            bool inPlace = Vector3.Distance(transform.position, _randomDestination) <= _distanceThreshold && 
                ActionStateMachine.CurrentState.Key != Idle;

            // check if in place and has a path
            if (inPlace && navMeshAgent.hasPath)
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
            if (_isAttackOnCooldown) return;
        
            if (targetEntity is IDamageable damageable)
            {
                damageable.TakeDamage(new DamageInfo(source: this, amount: _attackDamage));
            }

            StartCoroutine(AttackCounterDownTimer());
            /// set the rigid body of the enemy to kinematic
            rb.isKinematic = true;
            /// prevent the enemy from moving when in attack range
            navMeshAgent.isStopped = true;
            navMeshAgent.updateRotation = false;
        }

        void ActionDie()
        {
            // Clear the path to stop all movement
            navMeshAgent.ResetPath();

            // make the enemy ragdoll mode
            EnableRagdoll();

            // unsubscribe all state
            actionStateMachine.OnTransition -= OnActionTransition;

            actionStateMachine[Chase].EnableFixedUpdateCall = false;
            actionStateMachine[Chase].OnFixedUpdate -= OnChaseFixedUpdate;

            actionStateMachine[Roam].EnableFixedUpdateCall = false;
            actionStateMachine[Roam].OnFixedUpdate -= OnRoamFixedUpdate;

            Invoke(nameof(Kill), 5f); /// ragdoll despawn
        }

        void ActionChase()
        {
            /// set rigid body to dynamic   
            rb.isKinematic = false;

            /// allow enemy movement if not in attack range
            navMeshAgent.updateRotation = true;
            navMeshAgent.isStopped = false;

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