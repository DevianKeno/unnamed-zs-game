using System;
using UnityEngine;
using UZSG.Attributes;

namespace UZSG.Entities
{
    /// <summary>
    /// EntityData class made specifically for Player entities.
    /// </summary>
    [Serializable]
    [CreateAssetMenu(fileName = "EntityData", menuName = "URMG/Entity Data/Player Entity Data")]
    public class PlayerEntityData : EntityData
    {        
        [Header("Attributes")]
        public AttributeCollection<VitalAttribute> Vitals;
        public AttributeCollection<GenericAttribute> Generic;
    }
}