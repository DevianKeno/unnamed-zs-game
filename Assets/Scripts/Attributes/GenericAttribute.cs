using System;
using UnityEngine;

namespace UZSG.Attributes
{
    [Serializable]
    public class GenericAttribute : Attribute
    {
        public GenericAttribute(AttributeData data) : base(data)
        {
            this.data = data;
        }
    }
}
