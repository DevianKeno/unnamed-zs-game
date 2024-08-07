using System;
using System.Collections.Generic;
using UZSG.Attributes;
using UZSG.Crafting;

namespace UZSG.Data
{
    [Serializable]
    public struct EnemySaveData
    {
        public List<GenericAttributeSaveData> GenericAttributes;
    }
}
