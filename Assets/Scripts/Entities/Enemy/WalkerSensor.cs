using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using MEC;

using static UZSG.Entities.EnemyActionStates;

namespace UZSG.Entities
{
    /// Walker, agent sensor part.
    public partial class Walker
    {
        /// <summary>
        /// Set this Enemy's target to the detected player then show detection animation.
        /// </summary>
        /// <param name="player"></param>
        public override void NotifyDetection(Player player)
        {
            if (player == null) return;
            
            /// if enemy has target and is chasing a non null player
            if (this._hasTargetInSight)
            {
                /// results to target being locked to a single player
                return;
            }

            if (this.InRangeOf(player.Position, this._playerVisionRange))
            {
                _hasTargetInSight = true;
                targetEntity = player; 
                /// Clear the path to stop all movement
                NavMeshAgent.ResetPath();
                BeginChasePhase();
            }
        }

        public void BeginChasePhase()
        {
            Timing.KillCoroutines(_idleRoutine); /// end idle routine
            /// Scream at player then chase
            if (!_hasAlreadyScreamed)
            {
                _hasAlreadyScreamed = true;
                Timing.RunCoroutine(_FacePlayerAndScream());
            }
        }

        public void PerformAttack(Entity etty)
        {
            if (_isAttacking || !_hasTargetInSight)
            {
                return;
            }

            if (etty is Player player)
            {
                _hasTargetInAttackRange = true;
                // Calculate the direction vector from your object to the target object
                Vector3 directionToTarget = (player.Position - transform.position).normalized;

                // Check if the forward direction of your object is aligned with the directionToTarget vector
                float angle = Vector3.Angle(transform.forward, directionToTarget);

                // if facing player attack, else rotate
                if (angle < _rotationThreshold) // Adjust the threshold (e.g., 1 degree) as needed
                {   
                    MoveStateMachine.ToState(EnemyMoveStates.Idle);
                    ActionStateMachine.ToState(Attack);
                }
                else
                {
                    if (!_isAlreadyRotating)
                    {
                        StartCoroutine(Rotate());
                    }
                }
            }
        }

        public void ResetTargetIfNotInRange()
        {
            /// Check if there is a target, then calculate the distance
            if (_hasTargetInSight)
            {
                var distanceFromPlayer = Vector3.Distance(targetEntity.Position, transform.position); 

                if (targetEntity.InRangeOf(this.Position, radius: _attackRange)) /// perform attack
                {
                    PerformAttack(null);
                    return;
                }
            }
            else
            {
                // if (targetEntity.InRangeOf(this.Position, radius: _attackRange))
                // {
                //     // if target no longer in site reset target and roam (idle state)
                //     if (_attackRange <= distanceFromPlayer)
                //     {
                //         targetEntity = null;
                //         _hasTargetInSight = false;
                //         _hasAlreadyScreamed = false;
                //         ActionStateMachine.ToState(Roam);
                //         MoveStateMachine.ToState(EnemyMoveStates.Walk);
                //     }
                //     else
                //     {
                //         // Check if no player in attack range, reset to chase
                //         if (_hasTargetInAttackRange)
                //         {
                //             if (_attackRange <= distanceFromPlayer)
                //             {   
                //                 // reset target and rotation
                //                 _hasTargetInAttackRange = false;

                //                 ActionStateMachine.ToState(Chase);
                //                 MoveStateMachine.ToState(EnemyMoveStates.Run);
                //             }
                //         }
                //     }
                // }
            }
        }

        IEnumerator<float> _FacePlayerAndScream()
        {
            /// Face player before screaming
            StartCoroutine(Rotate());

            /// Once facing the player, scream
            ActionStateMachine.ToState(Scream, lockForSeconds: 2f);
            
            // if no target found after screaming
            if (!_hasTargetInSight)
            {
                ActionStateMachine.ToState(Roam);
                yield break;
            }
            yield return Timing.WaitForSeconds(2.2f);

            _targetMoveSpeed = MoveSpeed * chaseMoveSpeedMultiplier;
            /// set rigid body to dynamic   
            // Rigidbody.isKinematic = false;
            /// allow enemy movement if not in attack range
            // NavMeshAgent.updateRotation = true;
            // NavMeshAgent.isStopped = false;
            
            /// just chase player
            ActionStateMachine.ToState(Chase);
            MoveStateMachine.ToState(EnemyMoveStates.Run);
        }

        IEnumerator Rotate()
        {
            _isAlreadyRotating = true;
            /// Rotate towards the player
            Quaternion targetRotation = Quaternion.LookRotation(targetEntity.Position - transform.position);
            while (Quaternion.Angle(transform.rotation, targetRotation) > _rotationThreshold)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * _rotationDamping);
                yield return null;
            }

            _isAlreadyRotating = false;
        }

        IEnumerator<float> _IdleRoutine()
        {
            yield return Timing.WaitForSeconds(GetRandomDurationSeconds(minIdleTime, maxIdleTime));

            ActionStateMachine.ToState(Roam);
            MoveStateMachine.ToState(EnemyMoveStates.Walk);
        }

        IEnumerator<float> _AttackCooldownTimer()
        {
            _isAttackOnCooldown = true;
            yield return Timing.WaitForSeconds(_attackCooldown);
            _isAttackOnCooldown = false;

            /// if there is a target in range idle, else if not in range chase
            if (_hasTargetInAttackRange)
            {
                ActionStateMachine.ToState(Idle);
            }
            else
            {
                ActionStateMachine.ToState(Chase);
                MoveStateMachine.ToState(EnemyMoveStates.Run);
            }
        }

        float GetRandomDurationSeconds(float min, float max)
        {
            return UnityEngine.Random.Range(min, max);
        }
    }
}