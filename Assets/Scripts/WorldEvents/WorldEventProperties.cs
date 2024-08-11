using System;
using System.Collections.Generic;
using UnityEngine;

namespace UZSG.WorldEvents
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
}