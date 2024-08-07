using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace UZSG.World
{
    [Serializable]
    public class WorldEventProperties
    {
        public WorldEventType Type;
        public bool Active;
        public float ChanceToOccur;
        public int OccurEverySecond;
        public List<EventPrefab> EventPrefab;
    }
    
    [Serializable]
    public struct EventPrefab
    {
        public string Name;
        public GameObject Prefab;
        public float ChanceToOccur;
    }

    [CreateAssetMenu(fileName = "New World Event Data", menuName = "UZSG/World Event Data")]
    [Serializable]
    public class WorldEventData : ScriptableObject
    {
        public WorldEventProperties worldEvents;
    }
}
