using System;
using System.Collections.Generic;

using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;

using UZSG.Crafting;

namespace UZSG.Items
{
    public enum ItemType { Item, Weapon, Tool, Equipment, Accessory }
    public enum ItemSubtype { None, Useable, Food, Consumable, Tool, Weapon, Equipable, Accessory }

    [Serializable]
    [CreateAssetMenu(fileName = "New Item Data", menuName = "UZSG/Item Data")]
    public class ItemData : BaseData
    {
        /// Item Attributes
        public string Name;
        [TextArea] public string Description;
        public AssetReference Model;
        public Sprite Sprite;
        public ItemType Type;
        public ItemSubtype Subtype;
        public float Weight;
        public bool IsStackable;
        public int StackSize;

        /// Crafting
        public bool IsMaterial;
        public bool IsCraftable;
        public List<RecipeData> Recipes;
    }
}