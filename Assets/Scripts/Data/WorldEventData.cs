using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

using UZSG.WorldEvents;

namespace UZSG.Data
{
    [CreateAssetMenu(fileName = "New World Event Data", menuName = "UZSG/World Event Data")]
    [Serializable]
    public class WorldEventData : ScriptableObject
    {
        public WorldEventProperties worldEvents;
    }
}