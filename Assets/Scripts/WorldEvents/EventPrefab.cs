using System;
using System.Collections.Generic;
using UnityEngine;

namespace UZSG.WorldEvents
{    
    [Serializable]
    public struct EventPrefab
    {
        public string Name;
        public GameObject Prefab;
        public float ChanceToOccur;

        public static explicit operator List<object>(EventPrefab v)
        {
            throw new NotImplementedException();
        }
    }
}