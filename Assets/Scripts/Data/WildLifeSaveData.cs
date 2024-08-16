using System;
using System.Collections.Generic;
using UZSG.Saves;

namespace UZSG.Data
{
    [Serializable]
    public class WildlifeSaveData : SaveData
    {
        public List<AttributeSaveData> Attributes;
    }
}
