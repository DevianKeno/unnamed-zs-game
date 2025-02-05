using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Serialization;
using UZSG.Systems;

namespace UZSG.Data
{
    public enum ItemType { 
        Item, Weapon, Tool, Equipment, Armor, Accessory, Tile
    }
    public enum ItemSubtype {
        None, Useable, Food, Consumable, Tool, Weapon, Equipable, Accessory
    }

    [Serializable]
    [CreateAssetMenu(fileName = "New Item Data", menuName = "UZSG/Items/Item Data")]
    public class ItemData : BaseData
    {
        [Header("Item Data")]
        [FormerlySerializedAs("Name")]
        public string DisplayName;
        public string DisplayNameTranslatable => Game.Locale.Translatable($"item.{Id}.name");
        [TextArea] public string Description;
        public string DescriptionTranslatable => Game.Locale.Translatable($"item.{Id}.description");
        [FormerlySerializedAs("Model")] public AssetReference EntityModel;
        public Sprite Sprite;
        public ItemType Type;
        public ItemSubtype Subtype;
        public float Weight;
        public bool IsStackable => StackSize > 1;
        public int StackSize = 1;
        public bool IsFuel;
        [FormerlySerializedAs("FuelDuration")]
        public int FuelDurationSeconds;
        /// <summary>
        /// If this Item is a placeable Object.
        /// </summary>
        public bool IsObject;
        public ObjectData ObjectData;

        /// Crafting
        public bool IsMaterial;
        public bool IsCraftable;
        public List<RecipeData> Recipes;
        [TextArea] public string SourceDescription;
        public string SourceDescriptionTranslatable => Game.Locale.Translatable($"item.{Id}.source");

        [Header("Audio Data")]
        public AudioAssetsData AudioAssetsData;
    }
}