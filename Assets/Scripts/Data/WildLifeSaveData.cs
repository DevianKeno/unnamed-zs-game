using System;
using System.Collections.Generic;
using UZSG.Saves;

namespace UZSG.Data
{
    [Serializable]
    public class WildLifeSaveData : SaveData
    {
        public List<AttributeSaveData> Attributes;
    }
}
