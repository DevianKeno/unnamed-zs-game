using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UZSG.Attributes;
using UZSG.Entities;
using UZSG.Systems;

namespace UZSG.Data
{
    [Serializable]
    [CreateAssetMenu(fileName = "New Enemy Data", menuName = "UZSG/Entity/Enemy Data")]
    public class EnemyData : EntityData
    {
        public AttributeCollection<GenericAttribute> Generic;
    }
}