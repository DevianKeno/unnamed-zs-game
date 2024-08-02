using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UZSG.Entities;


namespace UZSG.World
{
    [Serializable]
    public struct EventPrefab
    {
        public string Name;
        public GameObject Prefab;
        public float ChanceToOccur;
    }

    [Serializable]
    [CreateAssetMenu(fileName = "New World Event Data", menuName = "UZSG/World Event Data")]
    public class WorldEventData : BaseData
    {
        public string Name;
        public float ChanceToOccur;
        public int OccurEverySecond;

        public List<EventPrefab> EventPrefab;
    }

    public class RaidPartyEventData : WorldEventData
    {
        public List<EntityData> EntitiesToSpawn;
    }
}
