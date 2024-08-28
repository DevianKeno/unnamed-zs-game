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

        /*public void AttackPlayer(Entity etty)
        {
            if (etty != null && etty is Player player)
            {
                actionStateMachine.ToState(EnemyActionStates.Attack);
            }
        }*/

        public void ResetTargetIfNotInRange()
        {
            // Check if there is a target, then calculate the distance
            if (_hasTargetInSight)
            {
                _distanceFromPlayer = Vector3.Distance(targetEntity.Position, transform.position); 
        
                if (_siteRadius <= _distanceFromPlayer) // if target no longer in site reset target
                {
                    targetEntity = null;
                    _hasTargetInSight = false;
                    _hasAlreadyScreamed = false;
                }
                else
                {
                    // Check if no player in attack range, reset to chase
                    if (_hasTargetInAttackRange)
                    {
                        if (_attackRadius <= _distanceFromPlayer)
                        {

                        }
                    }
                }
            }
        }

        #endregion

    }
}