using System;
using UnityEngine;
using UnityEngine.AI;
using UZSG.Data;
using UZSG.Systems;

namespace UZSG.Entities
{
    public abstract class Enemy : Entity
    {
        private float distanceFromPlayer; // the actual distance in game distance from the player
        public float siteRange;  // range from which it follow players
        public Transform player; // used for player position
        [SerializeField] private NavMeshAgent enemyEntity; // the entity's agent movement
        public float roamRadius; // Radius of which the agent can travel
        public float roamInterval; // Interval before the model moves again
        public float roamTime; // Time it takes for the agent to travel a point
        private Vector3 randomDestination; // Destination of agent
        public EnemyData EnemyData => entityData as EnemyData;
        [SerializeField] protected EnemyActionStatesMachine enemyStateMachine;

        public EnemyActionStatesMachine EnemyStateMachine => enemyStateMachine;

        void Start()
        {
            enemyEntity = GetComponent<NavMeshAgent>();
        }

        // SENSOR METHODS
        public EnemyActionStates _isInSiteRange()  // determines if enemy can chase player or roam map
        {
            distanceFromPlayer = Vector3.Distance(player.position, transform.position);
            if (distanceFromPlayer > siteRange)
            {
                return EnemyActionStates.Roam;
            }
            return EnemyActionStates.Chase;
        }

        public bool _isInAttackrange() // determines if enemy can attack player
        {
            return false;
        }
        public bool _isNoHealth() // determines if the enemy is dead
        {
            return false;
        }
        public bool _isSpecialAttackTriggered() // determines if an event happened that triggered special attack 1
        {
            return false;
        }
        public bool _isSpecialAttackTriggered2() // determines if an event happened that triggered special attack 2
        {
            return false;
        }

        public EnemyActionStates handleTransition() // "sense" what's the state of the enemy 
        {
            // if enemy has no health, state is dead
            if (_isNoHealth() == true)
            {
                return EnemyActionStates.Die;
            }
            // if player not in chase range
            if (_isInSiteRange() == EnemyActionStates.Roam)
            {
                return EnemyActionStates.Roam;
            }
            else
            {
                if (_isInAttackrange() == false) // chase
                {
                    return EnemyActionStates.Chase;
                }
                else // attack player
                {
                    return EnemyActionStates.Attack;
                }
            }
        }

        // ACTION METHODS
        public void chase()
        {
            enemyStateMachine.ToState(EnemyActionStates.Chase);
            enemyEntity.SetDestination(player.position);
            Debug.Log("distance from player: " + distanceFromPlayer + "with a trigger needed of: " + siteRange);
        }
        public void roam()
        {
            enemyStateMachine.ToState(EnemyActionStates.Roam);
            roamTime -= Time.deltaTime;
            if (roamTime <= 0)
            {
                // Get a random position
                randomDestination = UnityEngine.Random.insideUnitSphere * roamRadius;
                randomDestination += transform.position;

                NavMesh.SamplePosition(randomDestination, out NavMeshHit navHit, roamRadius, NavMesh.AllAreas);

                // Set the agent's destination to the random point
                enemyEntity.SetDestination(navHit.position);
                Debug.Log("distance from player: " + navHit.position);
                roamTime = UnityEngine.Random.Range(1.0f, roamInterval);
            }
        }
        public void attack2() 
        {
            enemyStateMachine.ToState(EnemyActionStates.Attack2);
            Debug.Log("attack2"); 
        }
        public void attack()
        {
            enemyStateMachine.ToState(EnemyActionStates.Attack);
            Debug.Log("attack"); 
        }
        public void die()
        {
            enemyStateMachine.ToState(EnemyActionStates.Die);
            Debug.Log("die"); 
        }
        public void specialAttack()
        {
            enemyStateMachine.ToState(EnemyActionStates.SpecialAttack);
            Debug.Log("specialAttack"); 
        }
        public void specialAttack2()
        {
            enemyStateMachine.ToState(EnemyActionStates.SpecialAttack2);
            Debug.Log("specialAttack2"); 
        }

        public void executeAction(EnemyActionStates action) // execute an action depending on what state the entity is on
        {
            switch (action)
            {
                case EnemyActionStates.Chase:
                    chase();
                    break;
                case EnemyActionStates.Roam:
                    roam();
                    break;
                case EnemyActionStates.Attack2:
                    attack2();
                    break;
                case EnemyActionStates.Attack:
                    attack();
                    break;
                case EnemyActionStates.Die:
                    die();
                    break;
                case EnemyActionStates.SpecialAttack:
                    specialAttack();
                    break;
                case EnemyActionStates.SpecialAttack2:
                    specialAttack2();
                    break;
            }
        }
    }
}