using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


namespace UZSG.Entities
{
    public class EnemyMovement : MonoBehaviour
    {
        //public Entity entity;
        public Transform player; // used for player position
        private NavMeshAgent _enemyEntity; // the entity's agent movement
        public float triggerDistance; // minimum distance from player before entity follows it
        private float _distanceFromPlayer; // the actual distance in game distance from the player
        public float RoamRadius; // Radius of which the agent can travel
        public float RoamInterval; // Interval before the model moves again
        public float RoamTime; // Time it takes for the agent to travel a point
        private Vector3 _randomDestination; // Destination of agent

        // Start is called before the first frame update
        void Start()
        {
            // Set the movement and world finding of the agent
            _enemyEntity = GetComponent<NavMeshAgent>();
        }

        // Update is called once per frame
        void LateUpdate()
        {
            _distanceFromPlayer = Vector3.Distance(player.position, transform.position);
            // Chase player if in range
            if (_distanceFromPlayer < triggerDistance)
            {
                _enemyEntity.SetDestination(player.position);
                Debug.Log("distance from player: " + _distanceFromPlayer + "with a trigger needed of: " + triggerDistance);    
            }
            // Roam if not
            else
            {
                RoamTime -= Time.deltaTime;
                if (RoamTime <= 0)
                {
                    Roam();
                    RoamTime = UnityEngine.Random.Range(1.0f, RoamInterval);
                }
            }
        }

        void Roam()
        {
            // Get a random position
            _randomDestination = UnityEngine.Random.insideUnitSphere * RoamRadius;
            _randomDestination += transform.position;

            NavMeshHit navHit;
            NavMesh.SamplePosition(_randomDestination, out navHit, RoamRadius, NavMesh.AllAreas);

            // Set the agent's destination to the random point
            _enemyEntity.SetDestination(navHit.position);
            Debug.Log("distance from player: " + navHit.position);
        }
    }
}
