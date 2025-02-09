using System;
using System.Collections.Generic;

using UnityEngine;

namespace UZSG.Data
{
    /// <summary>
    /// Base data for all data-able stuff.
    /// Values are set in Inspector; <b>Do not write</b>.
    /// </summary>
    [Serializable]
    public class BaseData : ScriptableObject
    {
        /// <summary>
        /// Source namespace. [Data, Do Not Write]
        /// </summary>
        public string Namespace = "uzsg";
        /// <summary>
        /// Unique identifier. [Data, Do Not Write]
        /// </summary>
        public string Id;
        /// <summary>
        /// Unused rn. [Data, Do Not Write]
        /// </summary>
        public List<string> Tags;
    }
}