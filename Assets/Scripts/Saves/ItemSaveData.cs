using System;
using System.Collections.Generic;

using UZSG.Attributes;

namespace UZSG.Saves
{
    public class ItemSaveData
    {
        public string Id = "none";
        public int Count = 0;
        /// <summary>
        /// If this Item is not a generic Item (e.g., Tool, Weapon), meaning it has Attributes.
        /// </summary>
        public bool HasAttributes = false;
        public List<AttributeSaveData> Attributes = new();
    }
}