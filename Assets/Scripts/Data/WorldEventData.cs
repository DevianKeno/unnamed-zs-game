using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace UZSG.World
{
    [Serializable]
    public class WorldEvent
    {
        public void StartEvent()
        {
            OnEventStart?.Invoke(this, EventType);
        }
        public string EventType;
        public bool EventOngoing;
        public float ChanceToOccur;
        public bool Active;
        public int OccurEverySecond;
        public event EventHandler<string> OnEventStart;
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
        public WorldEvent worldEvents;
    }
}
