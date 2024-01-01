using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UZSG.Systems
{
    public class PlayerSpawner : MonoBehaviour
    {
        public GameObject playerPrefab;

        public void Spawn(string id)
        {
            var go = Instantiate(playerPrefab);
            go.name = "Player";
        }    
    }
}
