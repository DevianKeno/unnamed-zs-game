using System;
using System.Collections.Generic;

using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;

using UZSG.Crafting;

namespace UZSG.Items
{
    public enum ItemType { Item, Weapon, Tool, Equipment, Accessory }
    public enum ItemSubtype { None, Consumable, Tool, Weapon, Equipable, Accessory }

    [Serializable]
    [CreateAssetMenu(fileName = "New Item Data", menuName = "UZSG/Item Data")]
    public class ItemData : ScriptableObject
    {
        [Header("Item Attributes")]
        public AssetReference Model;
        public string Id;
        public string Name;
        [TextArea] public string Description;
        public Sprite Sprite;
        public ItemType Type;
        public int StackSize;
        public ItemSubtype Subtype;

        [Header("Crafting")]
        public bool IsMaterial;
        public bool IsCraftable;
        public List<RecipeData> Recipes;
    }
    
    [CustomEditor(typeof(ItemData))]
    public class ItemDataEditor : Editor
    {
        SerializedProperty assetReference,
            id,
            nameProperty,
            description,
            sprite,
            stackSize,
            type,
            subType,
            isMaterial;
        
        void OnEnable()
        {
            assetReference = serializedObject.FindProperty("AssetReference");
            id = serializedObject.FindProperty("Id");
            nameProperty = serializedObject.FindProperty("Name");
            description = serializedObject.FindProperty("Description");
            sprite = serializedObject.FindProperty("Sprite");
            stackSize = serializedObject.FindProperty("StackSize");
            type = serializedObject.FindProperty("Type");
            subType = serializedObject.FindProperty("SubType");
            isMaterial = serializedObject.FindProperty("IsMaterial");
        }
    }
}