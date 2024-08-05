using System;
using UnityEngine;
using UnityEngine.AI;
using UZSG.Data;
using UZSG.Systems;

namespace UZSG.Entities
{
    public abstract class Enemy : Entity
    {
        public float SiteRange;  // range from which it follow Players
        public Transform Player; // used for Player position
        public float RoamRadius; // Radius of which the agent can travel
        public float RoamInterval; // Interval before the model moves again
        public float RoamTime; // Time it takes for the agent to travel a point
        public EnemyActionStatesMachine EnemyStateMachine => enemyStateMachine;
        public EnemyData EnemyData => entityData as EnemyData;
        Vector3 _randomDestination; // Destination of agent
        float distanceFromPlayer; // the actual distance in game distance from the Player
        [SerializeField] protected EnemyActionStatesMachine enemyStateMachine;
        [SerializeField] private NavMeshAgent _enemyEntity; // the entity's agent movement

        void Start()
        {
            _enemyEntity = GetComponent<NavMeshAgent>();
        }

        #region Agent sensors
        public EnemyActionStates IsInSiteRange  // determines if enemy can Chase Player or Roam map
        {
            get
            {
                if (distanceFromPlayer > SiteRange)
                {
                    return EnemyActionStates.Roam;
                }
                return EnemyActionStates.Chase;
            }
        }

        public bool IsInAttackrange // determines if enemy can Attack Player
        {
            get
            {
                return false;
            }
        }
        public bool IsNoHealth // determines if the enemy is dead
        {
            get
            {
                return false;
            }
        }
        public bool IsSpecialAttackTriggered // determines if an event happened that triggered special Attack 1
        {
            get
            {
                return false;
            }
        }
        public bool IsSpecialAttackTriggered2 // determines if an event happened that triggered special Attack 2
        {
            get
            {
                return false;
            }
        }

        public EnemyActionStates HandleTransition // "sense" what's the state of the enemy 
        {
            get
            {
                // if enemy has no health, state is dead
                if (IsNoHealth == true)
                {
                    return EnemyActionStates.Die;
                }
                // if Player not in Chase range
                if (IsInSiteRange == EnemyActionStates.Roam)
                {
                    return EnemyActionStates.Roam;
                }
                else
                {
                    if (IsInAttackrange == false) // Chase
                    {
                        return EnemyActionStates.Chase;
                    }
                    else // Attack Player
                    {
                        return EnemyActionStates.Attack;
                    }
                }
            }
        }

        #endregion


        #region Agent actuator

        public void Chase()
        {
            enemyStateMachine.ToState(EnemyActionStates.Chase);
            _enemyEntity.SetDestination(Player.position);
            Debug.Log("distance from Player: " + distanceFromPlayer + "with a trigger needed of: " + SiteRange);
        }
        public void Roam()
        {
            enemyStateMachine.ToState(EnemyActionStates.Roam);
            RoamTime -= Time.deltaTime;
            if (RoamTime <= 0)
            {
                // Get a random position
                _randomDestination = UnityEngine.Random.insideUnitSphere * RoamRadius;
                _randomDestination += transform.position;

                NavMesh.SamplePosition(_randomDestination, out NavMeshHit navHit, RoamRadius, NavMesh.AllAreas);

                // Set the agent's destination to the random point
                _enemyEntity.SetDestination(navHit.position);
                Debug.Log("distance from Player: " + navHit.position);
                RoamTime = UnityEngine.Random.Range(1.0f, RoamInterval);
            }
        }
        public void Attack2() 
        {
            enemyStateMachine.ToState(EnemyActionStates.Attack2);
            Debug.Log("Attack2"); 
        }
        public void Attack()
        {
            enemyStateMachine.ToState(EnemyActionStates.Attack);
            Debug.Log("Attack"); 
        }
        public void Die()
        {
            enemyStateMachine.ToState(EnemyActionStates.Die);
            Debug.Log("Die"); 
        }
        public void SpecialAttack()
        {
            enemyStateMachine.ToState(EnemyActionStates.SpecialAttack);
            Debug.Log("SpecialAttack"); 
        }
        public void SpecialAttack2()
        {
            enemyStateMachine.ToState(EnemyActionStates.SpecialAttack2);
            Debug.Log("SpecialAttack2"); 
        }

        public void executeAction(EnemyActionStates action) // execute an action depending on what state the entity is on
        {
            switch (action)
            {
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
            }
        }
        
        #endregion


    }
}