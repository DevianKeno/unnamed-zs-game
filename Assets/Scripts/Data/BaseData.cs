using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace UZSG.Data
{
    [Serializable]
    public class BaseData : ScriptableObject
    {
        [FormerlySerializedAs("SourceId")]
        public string Namespace = "uzsg";
        public string Id;
        public List<string> Tags;
    }
}