using System;
using System.Collections.Generic;

using UnityEngine;

using UZSG.Entities;
using UZSG.Systems;

namespace UZSG.Worlds.Events
{
    /// <summary>
    /// Natural enemy spawning. Subject to improvement.
    /// </summary>
    public class NaturalEnemySpawnEvent : WorldEventBase
    {
        const int MAX_NATURAL_ENEMY_SPAWN_COUNT = 16; /// TODO: scale with world difficulty
        /// <summary>
        /// Around the player 
        /// </summary>.
        const float MIN_SPAWN_RADIUS = 48; /// TODO: arbitrary value, change by gameplay design
        /// <summary>
        /// Around the player 
        /// </summary>.
        const float MAX_SPAWN_RADIUS = 96; /// TODO: arbitrary value, change by gameplay design
        /// <summary>
        /// Chance to spawn every second. Value between 0 - 100%.
        /// </summary>
        const int NATURAL_ENEMY_SPAWN_CHANCE = 10; /// TODO: arbitrary value, change by gameplay design
        readonly string[] enemyIds = { "walker" };

        bool allowNaturalSpawning = true;
        internal List<Enemy> spawnedEnemies = new();
        public int CurrentEnemyCount => spawnedEnemies.Count; 

        public NaturalEnemySpawnEvent() : base(0) /// this event is indefinite
        {
        }

        public override void OnStart()
        {
            Game.Tick.OnSecond += OnSecond;
        }

        public void OnSecond(SecondInfo s)
        {
            if (!allowNaturalSpawning) return;
            if (CurrentEnemyCount > MAX_NATURAL_ENEMY_SPAWN_COUNT) return;
            
            var chance = UnityEngine.Random.Range(0, 101);
            if (chance <= NATURAL_ENEMY_SPAWN_CHANCE)
            {
                SpawnRandomEnemy();
            }
        }

        void SpawnRandomEnemy()
        {
            if (Game.World.CurrentWorld.Players.Count <= 0) return;

            var position = GetRandomPositionAroundPlayer(GetRandomPlayer());

            Game.Entity.Spawn(GetRandomEnemy(), position, callback: (info) =>
            {
                if (info.Entity is not Enemy enemy) return;
                
                enemy.OnDeath += OnEnemyDeath;
                spawnedEnemies.Add(enemy);
            });
        }

        void OnEnemyDeath(Enemy enemy)
        {
            if (spawnedEnemies.Contains(enemy))
            {
                spawnedEnemies.Remove(enemy);
            }
        }

        Player GetRandomPlayer()
        {
            var index = UnityEngine.Random.Range(0, Game.World.CurrentWorld.PlayerCount - 1);
            return Game.World.CurrentWorld.Players[index];
        }

        string GetRandomEnemy()
        {
            var index = UnityEngine.Random.Range(0, enemyIds.Length - 1);
            return enemyIds[index];
        }
        
        Vector3 GetRandomPositionAroundPlayer(Player player)
        {
            var angleRadius = 90f;
            // Define the angle range behind the player
            float playerForwardAngle = Mathf.Atan2(player.Forward.z, player.Forward.x) * Mathf.Rad2Deg;
            float minAngle = playerForwardAngle - 180f - angleRadius / 2f; // Behind the player
            float maxAngle = playerForwardAngle - 180f + angleRadius / 2f;

            // Generate a random angle within the defined range
            float randomAngle = UnityEngine.Random.Range(minAngle, maxAngle);
            float randomRadius = UnityEngine.Random.Range(MIN_SPAWN_RADIUS, MAX_SPAWN_RADIUS);

            // Convert the angle to a direction
            Vector2 direction = new(Mathf.Cos(randomAngle * Mathf.Deg2Rad), Mathf.Sin(randomAngle * Mathf.Deg2Rad));
            Vector2 point = direction * randomRadius;

            // Terrain cast to find the ground height
            if (Physics.Raycast(new Vector3(point.x, 300f, point.y), -Vector3.up, out var hit, 999f))
            {
                return new Vector3(point.x, hit.point.y, point.y);
            }
            else
            {
                return player.transform.position + new Vector3(point.x, 0, point.y);
            }
        }

        internal void IncludeSpawned(Enemy enemy)
        {
            if (enemy._isNaturallySpawned)
            {
                spawnedEnemies.Add(enemy);
            }
        }
    }
}