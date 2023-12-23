using System;
using UnityEngine;

namespace URMG.Items
{
    public enum ItemType { Item, Weapon, Tool, Equipment, Accessory }

    [CreateAssetMenu(fileName = "Item", menuName = "URMG/Item")]
    [Serializable]
    public class ItemData : ScriptableObject
    {
        [Header("Item Attributes")]
        public string Id;
        public string Name;
        [TextArea] public string Description;
        public Sprite Sprite;
        public int StackSize;
        public ItemType Type;
        public bool IsMaterial;
        public GameObject Model;
    }
}