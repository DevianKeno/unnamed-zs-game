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
    public partial class Enemy : NonPlayerCharacter
    {
        
        #region Agent Player Detection

        /// <summary>
        /// Set this Enemy's target to the detected player then show detection animation.
        /// </summary>
        /// <param name="etty"></param>
        public void DetectPlayer(Entity etty)
        {
            if (etty != null && etty is Player player && !_hasTargetInSight)
            {
                _hasTargetInSight = true;
                targetEntity = player; 

                // Scream at player then chase
                if (!_hasAlreadyScreamed)
                {
                    _hasAlreadyScreamed = true;
                    StartCoroutine(FacePlayerAndScream());
                }
            }
        }

        public void AttackPlayer(Entity etty)
        {
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
                    actionStateMachine.ToState(EnemyActionStates.Attack);
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
        
                if (_siteRadius <= _distanceFromPlayer) // if target no longer in site reset target and roam (idle state)
                {
                    targetEntity = null;
                    _hasTargetInSight = false;
                    _hasAlreadyScreamed = false;
                    actionStateMachine.ToState(EnemyActionStates.Roam);
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

                            actionStateMachine.ToState(EnemyActionStates.Chase);
                        }
                    }
                }
            }
        }

        #endregion




        #region Zombie Dying

        public void KillZombieIfDead()
        {
            if (IsDead)
            {
                actionStateMachine.ToState(EnemyActionStates.Die);
            }
        }

        #endregion

    }
}