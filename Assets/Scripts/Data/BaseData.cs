using System;
using System.Collections.Generic;
using UnityEngine;

namespace UZSG.Data
{
    [Serializable]
    public class BaseData : ScriptableObject
    {
        public string SourceId = "uzsg";
        public string Id;
        public List<string> Tags;
    }
}