using Unity.Netcode;
using UnityEngine;

using UZSG.Systems;
using UZSG.Worlds;

namespace UZSG.Network
{
    /// <summary>
    /// Links and syncs worlds through the network.
    /// </summary>
    public class WorldNetwork : NetworkBehaviour
    {
        public World World { get; set; }

        public override void OnNetworkSpawn()
        {
            World = FindObjectOfType<World>();
            if (World == null)
            {
                Game.Console.LogError($"[WorldNetworkObject]: Unable to find world, will fail to initialize.");
                return;
            }

            if (IsServer)
            {
                Game.Tick.OnSecond += OnSecond;
            }
        }

        public override void OnNetworkDespawn()
        {
            Game.Tick.OnSecond -= OnSecond;
        }

        void OnSecond(SecondInfo info)
        {
            SyncTimeClientRpc(
                World.Time.Hour,
                World.Time.Minute,
                World.Time.Second);
        }

        [ClientRpc]
        public void SyncTimeClientRpc(int hour, int minute, int second)
        {
            if (IsServer) return;
            
            if (Game.World.IsInWorld)
            {
                World.Time.SetTime(hour, minute, second);
            }
        }
    }
}