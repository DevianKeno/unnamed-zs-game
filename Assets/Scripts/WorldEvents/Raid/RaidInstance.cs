using System.Collections.Generic;

using UnityEngine;

using UZSG.Systems;
using UZSG.Data;
using UZSG.Entities;

namespace UZSG.Worlds.Events
{
    public struct RaidInstance
    {
        public string enemyId; 
        public RaidEventType raidType;
        public RaidFormation raidFormation;
        public int mobCount;
    }
}