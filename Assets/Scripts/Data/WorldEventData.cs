using System;
using System.Collections.Generic;

using UnityEngine;
using UZSG.Worlds;
using UZSG.Worlds.Events;

namespace UZSG.Data
{
    [Serializable]
    public struct RaidEventInstance
    {
        public string Name;
        public EnemyData EnemyData;
        public float ChanceToOccur;
    }

    [CreateAssetMenu(fileName = "New World Event Data", menuName = "UZSG/World Event Data")]
    [Serializable]
    public class WorldEventData : ScriptableObject
    {
        public WorldEventType Type;
        public bool Enabled;
        public bool AllowMultipleEvents;
        public float ChanceToOccur;
        public int OccurEverySecond;
        public List<WeatherEventInstance> WeatherTypes;
        public List<RaidEventInstance> RaidTypes;
    }
}