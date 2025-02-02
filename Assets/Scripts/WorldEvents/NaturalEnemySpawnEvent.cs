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
        const int MAX_NATURAL_ENEMY_SPAWN_COUNT = 16;
        /// <summary>
        /// Around the player 
        /// </summary>.
        const float MIN_SPAWN_RADIUS = 24; /// TODO: arbitrary value, change by gameplay design
        /// <summary>
        /// Around the player 
        /// </summary>.
        const float MAX_SPAWN_RADIUS = 64; /// TODO: arbitrary value, change by gameplay design
        /// <summary>
        /// Chance to spawn every second. Value between 0 - 100%.
        /// </summary>
        const int NATURAL_ENEMY_SPAWN_CHANCE = 10; /// TODO: arbitrary value, change by gameplay design
        readonly string[] enemyIds = { "walker" };

        bool allowNaturalSpawning = true;
        List<Enemy> spawnedEnemies = new();
        public int CurrentEnemyCount => spawnedEnemies.Count; 

        public NaturalEnemySpawnEvent() : base(0) /// this event is indefinite
        {
        }

        public override void OnStart()
        {
        }

        public override void OnSecond()
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
            var index = UnityEngine.Random.Range(0, Game.World.CurrentWorld.Players.Count - 1);
            return Game.World.CurrentWorld.Players[index];
        }

        string GetRandomEnemy()
        {
            var index = UnityEngine.Random.Range(0, enemyIds.Length - 1);
            return enemyIds[index];
        }
        
        Vector3 GetRandomPositionAroundPlayer(Player player)
        {
            float randomAngle = UnityEngine.Random.Range(0f, 360f);
            float randomRadius = UnityEngine.Random.Range(MIN_SPAWN_RADIUS, MAX_SPAWN_RADIUS);
            Vector2 randomPointAroundPlayer = new Vector2(Mathf.Cos(randomAngle), Mathf.Sin(randomAngle)) * randomRadius;
            return player.transform.position + new Vector3(randomPointAroundPlayer.x, 0, randomPointAroundPlayer.y);
        }
    }
}