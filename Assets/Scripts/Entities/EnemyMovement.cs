using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UZSG.Systems;

namespace UZSG.Entities
{
    public class EnemyMovement : MonoBehaviour
    {
        public Entity entity;
        public Transform player; // used for player position
        private NavMeshAgent enemyEntity; // the entity's agent movement
        public float triggerDistance; // minimum distance from player before entity follows it
        private float distanceFromPlayer; // the actual distance in game distance from the player
        public float roamRadius; // Radius of which the agent can travel
        public float roamInterval; // Interval before the model moves again
        public float roamTime; // Time it takes for the agent to travel a point
        private Vector3 randomDestination; // Destination of agent

        // Start is called before the first frame update
        void Start()
        {
            // Set the movement and world finding of the agent
            enemyEntity = GetComponent<NavMeshAgent>();
        }

        // Update is called once per frame
        void LateUpdate()
        {
            distanceFromPlayer = Vector3.Distance(player.position, transform.position);
            // chase player if in range
            if (distanceFromPlayer < triggerDistance)
            {
                enemyEntity.SetDestination(player.position);
                Debug.Log("distance from player: " + distanceFromPlayer + "with a trigger needed of: " + triggerDistance);    
            }
            // roam if not
            else
            {
                roamTime -= Time.deltaTime;
                if (roamTime <= 0)
                {
                    roam();
                    roamTime = UnityEngine.Random.Range(1.0f, roamInterval);
                }
            }
        }

        void roam()
        {
            // Get a random position
            randomDestination = UnityEngine.Random.insideUnitSphere * roamRadius;
            randomDestination += transform.position;

            NavMeshHit navHit;
            NavMesh.SamplePosition(randomDestination, out navHit, roamRadius, NavMesh.AllAreas);

            // Set the agent's destination to the random point
            enemyEntity.SetDestination(navHit.position);
            Debug.Log("distance from player: " + navHit.position);
        }
    }
}
