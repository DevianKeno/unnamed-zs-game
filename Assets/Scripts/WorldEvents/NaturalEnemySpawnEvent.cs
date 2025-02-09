using System;
using System.Collections.Generic;

using UnityEngine;

using UZSG.Entities;


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

        public override void OnEnd()
        {
            Game.Tick.OnSecond -= OnSecond;
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
            // Generate a random point in a 2D circle
            Vector2 randomPoint = UnityEngine.Random.insideUnitCircle.normalized; // Normalize to ensure consistent distance
            float randomRadius = UnityEngine.Random.Range(MIN_SPAWN_RADIUS, MAX_SPAWN_RADIUS);
            randomPoint *= randomRadius;

            // Offset the point relative to the player's position
            Vector3 positionAround = player.Position + new Vector3(randomPoint.x, 0, randomPoint.y);

            // Ensure the point is behind the player
            Vector3 directionToPlayer = (positionAround - player.Position).normalized;
            if (Vector3.Dot(directionToPlayer, player.Forward) > 0)
            {
                // If the point is in front of the player, flip it to the opposite side
                positionAround = player.Position - new Vector3(randomPoint.x, 0, randomPoint.y);
            }

            // Terrain cast to find the ground height
            if (Physics.Raycast(new Vector3(positionAround.x, 300f, positionAround.z), -Vector3.up, out var hit, 999f))
            {
                return hit.point; // Return the point on the terrain
            }
            else
            {
                return positionAround; // Fallback to the calculated position
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