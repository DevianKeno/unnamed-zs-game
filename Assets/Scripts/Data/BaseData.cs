using System;
using UnityEngine;

namespace UZSG.Data
{
    [Serializable]
    [CreateAssetMenu(fileName = "New Food Data", menuName = "UZSG/Food Data")]
    public class BaseData : ScriptableObject
    {
        public string Id;
    }
}