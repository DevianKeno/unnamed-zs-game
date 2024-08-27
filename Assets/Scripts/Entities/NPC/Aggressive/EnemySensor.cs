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
        #region Agent sensors
        
        /*/// <summary>
        /// determines if enemy can Chase Player or Roam map
        /// </summary>
        public bool HasTargetInSight 
        {
            get
            {
                return _hasTargetInSight;
            }
        }
        /// <summary>
        /// determines if enemy can Attack Player
        /// </summary>
        public bool HasTargetInAttackRange
        {
            get
            {   
                return _hasTargetInAttackRange;
            }
        }
        /// <summary>
        /// determines if the enemy is dead
        /// </summary>
        public bool HasNoHealth
        {
            get
            {
                return IsDead;
            }
        }
        /// <summary>
        /// determines if an event happened that triggered special Attack 1
        /// </summary>
        public bool IsSpecialAttackTriggered
        {
            get
            {
                return false;
            }
        }
        /// <summary>
        /// Whether if an event happened that triggered special Attack 2
        /// </summary>
        public bool IsSpecialAttack2Triggered
        {
            get
            {
                return false;
            }
        }

        public bool IsInHordeMode
        {
            get
            {   // TODO: palitan mo yung "jericho_method" sa method na nagrereturn ng bool; true if in hordemode (lalakad straight line), false if hindi horde mode zombie
                if (_isInHordeMode)
                {
                    if (HasTargetInSight)
                    {
                        return false;
                    }
                    hordeTransform.SetPositionAndRotation(new Vector3(1, 2, 1), Quaternion.Euler(0, 30, 0));
                    return true;
                }
                return false;
            }
        }

        /// <summary>
        /// "sense" what's the state of the enemy 
        /// </summary>
        /// <returns></returns>
        /*public EnemyActionStates HandleTransition()
        {
            if (IsDead)
            {
                return EnemyActionStates.Die;
            }

            // if (IsHordeMode)
            // {
            //     return EnemyActionStates.Horde;
            // }

            if (HasTargetInSight)
            {
                if (HasTargetInAttackRange) 
                {
                    return EnemyActionStates.Attack;
                }
                else /// keep chasing
                {
                    return EnemyActionStates.Chase;
                }
            }
            else
            {
                return EnemyActionStates.Roam;
            }
        }*/

        #endregion
    }
}