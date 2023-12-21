using System;
using UnityEngine;

namespace URMG.Items
{
    public enum ItemType { Item, Weapon, Tool, Equipment, Accessory }

    [CreateAssetMenu(fileName = "Item", menuName = "URMG/Item")]
    [Serializable]
    public class ItemData : ScriptableObject
    {
        public string Id;
        public string Name;
        [TextArea] public string Description;
        public Sprite Sprite;
        public int StackSize;
        public ItemType Type;
    }
}