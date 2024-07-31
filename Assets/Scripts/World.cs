using UnityEngine;
using UnityEngine.AddressableAssets;
using UZSG.Maps;
using UZSG.Systems;

namespace UZSG.Worlds
{
    public class World : MonoBehaviour
    {

        public void LoadMap(Map map)
        {
            // if (!AssetReference.IsSet(map.Level))
            // {
            //     return;
            // }


        }
        
        internal void Initialize()
        {
            Game.Tick.OnTick += Tick;
        }

        void Tick(TickInfo t)
        {

        }
    }
}
