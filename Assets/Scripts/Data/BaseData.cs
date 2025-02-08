using System;
using System.Collections.Generic;

using UnityEngine;

namespace UZSG.Data
{
    [Serializable]
    public class BaseData : ScriptableObject
    {
        /// <summary>
        /// Source namespace.
        /// </summary>
        public string Namespace;
        /// <summary>
        /// Unique identifier.
        /// </summary>
        public string Id;
        /// <summary>
        /// Unused rn.
        /// </summary>
        public List<string> Tags;
    }
}