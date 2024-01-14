using System.Collections.Generic;
using UnityEngine;
using UZSG.Attributes;

namespace UZSG.PlayerCore
{
    [System.Serializable]
    public class PlayerGenericAttributes
    {
        Dictionary<string, Attribute> _attributes;

        public Attribute MoveSpeed;

        public Attribute this[string name]
        {
            get => _attributes[name];
        }
        
        internal void Initialize()
        {
            
        }
    }
}
