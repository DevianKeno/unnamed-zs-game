using System;
using UnityEngine;
using UZSG.Attributes;

namespace UZSG.Entities
{
    /// <summary>
    /// EntityData class made specifically for Player entities.
    /// </summary>
    [Serializable]
    [CreateAssetMenu(fileName = "New Player Entity Data", menuName = "UZSG/Entity/Player Entity Data")]
    public class PlayerEntityData : EntityData
    {        
        [Header("Attributes")]
        public AttributeCollection<VitalAttribute> Vitals;
        public AttributeCollection<GenericAttribute> Generic;
    }
}