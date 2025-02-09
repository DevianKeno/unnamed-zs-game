using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Serialization;

namespace UZSG.Data
{
    public enum ItemType { 
        Item, Weapon, Tool, Equipment, Armor, Accessory, Tile
    }
    public enum ItemSubtype {
        None, Useable, Food, Consumable, Tool, Weapon, Equipable, Accessory
    }

    /// <summary>
    /// Item data.
    /// Values are set in Inspector; <b>Do not write</b>.
    /// </summary>
    [Serializable]
    [CreateAssetMenu(fileName = "New Item Data", menuName = "UZSG/Items/Item Data")]
    public class ItemData : BaseData
    {
        [Header("Item Data")]
        [FormerlySerializedAs("Name")] public string DisplayName;
        public string DisplayNameTranslatable => Game.Locale.Translatable($"item.{Id}.name");

        [TextArea] public string Description;
        public string DescriptionTranslatable => Game.Locale.Translatable($"item.{Id}.description");

        public AssetReference EntityModel;
        public Sprite Sprite;
        public ItemType Type;
        public ItemSubtype Subtype;
        public float Weight;
        public bool IsStackable => StackSize > 1;
        public int StackSize = 1;
        /// <summary>
        /// Whether this item can be used as a fuel source.
        /// </summary>
        public bool IsFuel;
        [FormerlySerializedAs("FuelDuration")] public int FuelDurationSeconds;

        /// <summary>
        /// If this Item is a placeable Object.
        /// </summary>
        public bool IsObject;
        /// <summary>
        /// The object that this item represents.
        /// </summary>
        public ObjectData ObjectData;
        
        [Header("Crafting")]
        /// <summary>
        /// Whether this item is used to craft other items.
        /// </summary>
        public bool IsMaterial;
        /// <summary>
        /// Whether this item is can be crafted.
        /// </summary>
        public bool IsCraftable;
        /// <summary>
        /// List of recipes that outputs this item.
        /// </summary>
        public List<RecipeData> Recipes;

        /// <summary>
        /// About where this item can be gathered/taken from.
        /// </summary>
        [TextArea] public string SourceDescription;
        public string SourceDescriptionTranslatable => Game.Locale.Translatable($"item.{Id}.source");
        

        [Header("Audio Data")]
        public AudioAssetsData AudioAssetsData;
    }
}