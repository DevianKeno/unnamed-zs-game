using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ZS
{
    /// <summary>
    /// Generic attributes represent standard player stats.
    /// </summary>
    public abstract class GenericAttribute : Attribute
    {
        public override float BaseMaximum => throw new System.NotImplementedException();
    }
}
