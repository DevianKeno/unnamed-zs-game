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
    [CreateAssetMenu(fileName = "New Vehicle Data", menuName = "UZSG/Entity/Vehicle Data")]
    public class VehicleData : BaseData
    {
        public List<AttributeData> Generic;
    }
}
