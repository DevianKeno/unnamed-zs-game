using System;
using UnityEngine;
using UZSG.Data;

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
