using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UZSG.Systems;

namespace UZSG.Entities
{
    public class moves: Entity
    {

        public Transform player; // used for player position
        public float triggerDistance; // minimum distance from player before entity follows it
        private NavMeshAgent enemyEntity; // the entity's agent movement
        private float distanceFromPlayer; // the actual distance in game distance from the player
        
        [SerializeField] LayerMask groundLayer;
        [SerializeField] float defaultWalkRange; // how far can the entity walk

        void Start()
        {
            // Set the movement and world finding of the agent
            enemyEntity = GetComponent<NavMeshAgent>();
        }

        void Update() 
        {
            distanceFromPlayer = Vector3.Distance(player.position, transform.position);
            // entity chases the player
            if (distanceFromPlayer <= triggerDistance)
            {
                Debug.Log("following at: " + distanceFromPlayer + " with triggers needed of: " + triggerDistance);
                enemyEntity.SetDestination(player.position);
            }
        }

        void patrol()
        {
            Debug.Log("patrolling");
        }
    }
}