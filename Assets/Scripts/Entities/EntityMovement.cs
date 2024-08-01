using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UZSG.Systems;

namespace UZSG.Entities
{
    public class EntityMovement : Entity
    {
        public Transform player; // used for player position
        private NavMeshAgent enemyEntity; // the entity's agent movement
        public float triggerDistance; // minimum distance from player before entity follows it
        private float distanceFromPlayer; // the actual distance in game distance from the player

        // Start is called before the first frame update
        void Start()
        {
            // Set the movement and world finding of the agent
            enemyEntity = GetComponent<NavMeshAgent>();
        }

        // Update is called once per frame
        void LateUpdate()
        {
            enemyEntity.SetDestination(player.position);

            //distanceFromPlayer = Vector3.Distance(player.position, transform.position);
        }
    }
}
