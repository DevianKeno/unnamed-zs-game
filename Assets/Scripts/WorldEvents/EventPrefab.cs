using System;
using UnityEngine;

namespace UZSG.WorldEvents
{    
    [Serializable]
    public struct EventPrefab
    {
        public string Name;
        public GameObject Prefab;
        public float ChanceToOccur;
    }
}