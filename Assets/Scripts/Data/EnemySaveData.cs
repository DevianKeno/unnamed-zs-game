using System;
using UZSG.Saves;

namespace UZSG.Data
{
    [Serializable]
    public class EnemySaveData : SaveData
    {
        public AttributeCollectionSaveData GenericAttributes;
    }
}
