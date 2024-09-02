using System;
using UnityEngine;
using UZSG.Data;

namespace UZSG.Data
{
    [Serializable]
    [CreateAssetMenu(fileName = "Status Effect", menuName = "UZSG/Status Effect")]
    public class StatusEffectData : BaseData
    {
        public string Name;
        [TextArea] public string Description;
    }
}