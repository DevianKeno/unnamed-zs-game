using UnityEngine;
using static UZSG.Entities.EnemyActionStates;
using System.Collections;

namespace UZSG.Entities
{
    public partial class Enemy : NonPlayerCharacter
    {
        
        #region Agent Player Detection

        /// <summary>
        /// Set this Enemy's target to the detected player then show detection animation.
        /// </summary>
        /// <param name="etty"></param>
        public void DetectPlayer(Entity etty)
        {
            // if enemy is alive
            if (!IsDead)
            {
                // if enemy has target and is chasing a non null player
                if (!_hasTargetInSight && etty != null && etty is Player player)
                {
                    _hasTargetInSight = true;
                    targetEntity = player; 

                    // Clear the path to stop all movement
                    navMeshAgent.ResetPath();

                    // Scream at player then chase
                    if (!_hasAlreadyScreamed)
                    {
                        _hasAlreadyScreamed = true;
                        StartCoroutine(FacePlayerAndScream());
                    }
                }
            }
        }

        public void AttackPlayer(Entity etty)
        {
            if (isAttacking)
            {
                return;
            }
            if (etty != null && etty is Player player)
            {
                _hasTargetInAttackRange = true;
                // Calculate the direction vector from your object to the target object
                Vector3 directionToTarget = (player.Position - transform.position).normalized;

                // Check if the forward direction of your object is aligned with the directionToTarget vector
                float angle = Vector3.Angle(transform.forward, directionToTarget);

                // if facing player attack, else rotate
                if (angle < rotationThreshold) // Adjust the threshold (e.g., 1 degree) as needed
                {   
                    moveStateMachine.ToState(EnemyMoveStates.Idle);
                    actionStateMachine.ToState(Attack);
                }
                else
                {
                    if (!isAlreadyRotating)
                    {
                        StartCoroutine(Rotate());
                    }
                }
            }
        }

        public void ResetTargetIfNotInRange()
        {
            // Check if there is a target, then calculate the distance
            if (_hasTargetInSight)
            {
                _distanceFromPlayer = Vector3.Distance(targetEntity.Position, transform.position); 

                // if target no longer in site reset target and roam (idle state)
                if (_siteRadius <= _distanceFromPlayer)
                {
                    targetEntity = null;
                    _hasTargetInSight = false;
                    _hasAlreadyScreamed = false;
                    actionStateMachine.ToState(Roam);
                    moveStateMachine.ToState(EnemyMoveStates.Walk);
                }
                else
                {
                    // Check if no player in attack range, reset to chase
                    if (_hasTargetInAttackRange)
                    {
                        if (_attackRadius <= _distanceFromPlayer)
                        {   
                            // reset target and rotation
                            _hasTargetInAttackRange = false;

                            actionStateMachine.ToState(Chase);
                            moveStateMachine.ToState(EnemyMoveStates.Run);
                        }
                    }
                }
            }
        }

        #endregion


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
            yield return new WaitForSeconds(2.2f);

            /// just chase player
            actionStateMachine.ToState(Chase);
            moveStateMachine.ToState(EnemyMoveStates.Run);
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
            moveStateMachine.ToState(EnemyMoveStates.Walk);
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
                moveStateMachine.ToState(EnemyMoveStates.Run);
            }
        }


        #region Zombie Dying

        public void KillZombieIfDead()
        {
            if (IsDead)
            {
                moveStateMachine.ToState(EnemyMoveStates.Idle);
                actionStateMachine.ToState(Die);
            }
        }

        #endregion

    }
}