using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ZS
{
    /// <summary>
    /// Player health attribute.
    /// </summary>
    public class Health : VitalAttribute
    {
        public override float BaseMaximum
        {
            get { return Defaults.BaseMaxHealth; }
        }
    }
}
