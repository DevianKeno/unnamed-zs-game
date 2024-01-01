using System;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace UZSG.Items
{
    public enum ItemType { Item, Weapon, Tool, Equipment, Accessory }
    public enum ItemSubtype { Consumable, Tool, Weapon, Equipable, Accessory }

    [Serializable]
    [CreateAssetMenu(fileName = "Item", menuName = "URMG/Item")]
    public class ItemData : ScriptableObject
    {
        [Header("Item Attributes")]
        public string Id;
        public string Name;
        [TextArea] public string Description;
        public Sprite Sprite;
        public int StackSize;
        public ItemType Type;
        public ItemSubtype Subtype;
        public bool IsMaterial;
        public AssetReference AssetReference;
    }
}