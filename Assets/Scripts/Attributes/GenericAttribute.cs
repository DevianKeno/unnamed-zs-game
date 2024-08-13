using System;

using UnityEngine;
using UZSG.Data;
using UZSG.Saves;

namespace UZSG.Attributes
{
    [Serializable]
    public class GenericAttribute : Attribute
    {
        public GenericAttribute(AttributeData data) : base(data)
        {
        }
        
        public GenericAttribute(string id) : base(id)
        {
        }
        
        internal override void Initialize()
        {
            base.Initialize();
        }
        
        public virtual void ReadSaveJSON(GenericAttributeSaveData data, bool initialize = true)
        {
            value = data.Value;
            minimum = data.Minimum;
            baseMaximum = data.BaseMaximum;
            multiplier = data.Multiplier;
            flatBonus = data.FlatBonus;
            LimitOverflow = data.LimitOverflow;
            LimitUnderflow = data.LimitUnderflow;

            if (initialize)
            {
                Initialize();
            }
        }
    }
}
