using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UZSG.Entities;
using UZSG.Systems;

namespace UZSG.Minimap
{
    public class MinimapController : MonoBehaviour
    {
        public Player PlayerEntity;

        private void Start()
        {
            Game.Entity.OnEntitySpawned += OnSpawn;
        }

        private void OnSpawn(EntityManager.EntitySpawnedInfo info)
        {
            if (info.Entity is Player player)
            {
                PlayerEntity = player;

                Camera camera = GetComponent<Camera>();
                if (camera != null)
                {
                    Debug.Log("Camera component found, enabling it.");
                    camera.enabled = true;

                    // Optional: Print the camera status to ensure it's enabled
                    Debug.Log("Camera enabled status: " + camera.enabled);
                }
                else
                {
                    Debug.LogWarning("Camera component not found on this GameObject.");
                }
            }
        }

        // Update is called once per frame
        void Update()
        {
            transform.position = new Vector3(PlayerEntity.transform.position.x, PlayerEntity.transform.position.y + 80, PlayerEntity.transform.position.z);
        }
    }
}

