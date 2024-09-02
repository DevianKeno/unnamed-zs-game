using System.Collections.Generic;
using UnityEngine;
using UZSG.Entities;
using UZSG.Systems;

namespace UZSG.WorldEvents.Raid
{
    public class HordeFormations : MonoBehaviour
    {
        Player player;
        List<IEnemy> _hordeZombies = new();
        public List<IEnemy> HordeZombies => _hordeZombies;
        RaidInstance _raidInstance;
        Quaternion? _facingDirection = null;
        Vector3? _selectedPoint = null;

        /// Temporary values <summary>
        float minRadius = 30f;
        float maxRadius = 50f;
        float spread = 0.75f;

        public void HandlePrerequisites(RaidInstance raidInstance, Player selectedPlayer)
        {
            player = selectedPlayer;
            _raidInstance = raidInstance;

            switch(_raidInstance.raidFormation)
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
            for (int i = 0; i < _raidInstance.mobCount; i++)
            {
                print("Spawning zombie");
                SpawnZombie(GetRandomPositionAroundPlayer(_raidInstance.mobCount * spread));
            }

            _facingDirection = null;
            _selectedPoint = null;
        }

        void SpawnAsLine()
        {
            // Vector3 lineStart = player.transform.position + player.transform.right * -10f;
            // Vector3 lineEnd = player.transform.position + player.transform.right * 10f;

            // for (int i = 0; i < raidInstance.mobCount; i++)
            // {
            //     float t = i / (float)(raidInstance.mobCount - 1);
            //     Vector3 spawnPosition = Vector3.Lerp(lineStart, lineEnd, t);
            //     spawnPosition += player.transform.forward * 10f;
            //     SpawnZombie(spawnPosition);
            // }
            throw new System.NotImplementedException();
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
            Game.Entity.Spawn<Skinwalker>(_raidInstance.enemyId, position, callback: (info) => {
                _hordeZombies.Add(info.Entity);
                FaceTowardsPlayer(info.Entity);
            });
        }

        Vector3 GetRandomPositionAroundPlayer(float spreadRadius)
        {
            if (_selectedPoint == null)
            {
                float randomAngle = Random.Range(0f, 360f);
                float randomRadius = Random.Range(minRadius, maxRadius);
                Vector2 randomPointAroundPlayer = new Vector2(Mathf.Cos(randomAngle), Mathf.Sin(randomAngle)) * randomRadius;
                _selectedPoint = player.transform.position + new Vector3(randomPointAroundPlayer.x, 0, randomPointAroundPlayer.y);
            }

            Vector2 randomSpread = Random.insideUnitCircle * spreadRadius;
            Vector3 position = _selectedPoint.Value + new Vector3(randomSpread.x, 0, randomSpread.y);

            return position;
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