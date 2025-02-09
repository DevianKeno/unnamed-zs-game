using UnityEngine;

using UZSG.Entities;

namespace UZSG
{
    public class PlayerReference : MonoBehaviour
    {
        public Player PlayerEntity;

        void Start()
        {
            Game.Entity.OnEntitySpawned += OnSpawn;
        }

        void OnSpawn(EntityManager.EntityInfo info)
        {
            if(info.Entity is Player player)
            {
                PlayerEntity = player;
            }
        }
    }
}
