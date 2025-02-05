using System.Collections.Generic;

using UnityEngine;

using UZSG.Entities;
using UZSG.Systems;

namespace UZSG.Worlds.Events
{
    public class HordeFormations
    {
        Player player;
        List<Enemy> _hordeZombies = new();
        public List<Enemy> HordeZombies => _hordeZombies;
        RaidInstance _raidInstance;
        Quaternion _facingDirection = default;
        Vector3 _selectedPoint = default;

        /// Temporary values
        float minRadius = 30f;
        float maxRadius = 50f;
        float spread = 0.25f;

        public void HandlePrerequisites(RaidInstance raidInstance, Player selectedPlayer)
        {
            player = selectedPlayer;
            _raidInstance = raidInstance;

            switch(_raidInstance.RaidFormation)
            {
                case RaidFormation.Blob:
                    SpawnAsBlob();
                    break;
                case RaidFormation.Line:
                    SpawnAsLine();
                    break;
                case RaidFormation.Waves:
                    SpawnInWaves();
                    break;
            }
        }

        void SpawnAsBlob()
        {
            GetRandomPositionAroundPlayer();
            float spreadRadius = _raidInstance.MobCount * spread;

            for (int i = 0; i < _raidInstance.MobCount; i++)
            {
                Vector2 randomSpread = Random.insideUnitCircle * spreadRadius;
                Vector3 position = _selectedPoint + new Vector3(randomSpread.x, 0, randomSpread.y);
                SpawnZombie(position);
            }

            _facingDirection = default;
            _selectedPoint = default;
        }

        void SpawnAsLine()
        {
            GetRandomPositionAroundPlayer();
            Vector3 lineStart = _selectedPoint;
            Vector3 lineDirection = (player.transform.position - lineStart).normalized;

            for (int i = 0; i < _raidInstance.MobCount; i++)
            {
                float distanceAlongLine = i * spread;
                Vector3 linePosition = lineStart + lineDirection * distanceAlongLine;

                Vector2 randomSpread = Random.insideUnitCircle * spread;
                Vector3 spawnPosition = linePosition + new Vector3(randomSpread.x, 0, randomSpread.y);

                SpawnZombie(spawnPosition);
            }

            _facingDirection = default;
            _selectedPoint = default;
        }

        void SpawnInWaves()
        {
            // int wavesCount = 3;
            // int zombiesPerWave = raidInstance.mobCount / wavesCount;

            // for (int wave = 0; wave < wavesCount; wave++)
            // {
            //     float radius = 10f + wave * 5f; // Increasing radius for each wave
            //     for (int i = 0; i < zombiesPerWave; i++)
            //     {
            //         float angle = i * (360f / zombiesPerWave);
            //         Vector3 spawnPosition = player.transform.position + Quaternion.Euler(0, angle, 0) * Vector3.forward * radius;
            //         SpawnZombie(spawnPosition);
            //     }
            // }
            throw new System.NotImplementedException();
        }

        void SpawnZombie(Vector3 position)
        {
            Game.Entity.Spawn<Walker>(_raidInstance.EnemyId, position, onCompleted: (info) => {
                _hordeZombies.Add(info.Entity);
                FaceTowardsPlayer(info.Entity);
                // info.Entity.isInHorde = true;
            });
        }

        void GetRandomPositionAroundPlayer()
        {
            if (_selectedPoint == null)
            {
                float randomAngle = Random.Range(0f, 360f);
                float randomRadius = Random.Range(minRadius, maxRadius);
                Vector2 randomPointAroundPlayer = new Vector2(Mathf.Cos(randomAngle), Mathf.Sin(randomAngle)) * randomRadius;
                _selectedPoint = player.transform.position + new Vector3(randomPointAroundPlayer.x, 0, randomPointAroundPlayer.y);
            }
        }

        void FaceTowardsPlayer(Entity entity)
        {
            if (_facingDirection == null)
            {
                _facingDirection = Quaternion.LookRotation(player.Position - entity.Position);
            }

            entity.Rotation = (Quaternion)_facingDirection;
        }
    }
}