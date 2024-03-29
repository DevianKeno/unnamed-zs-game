using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace UZSG.Items
{
    public enum ItemType { Item, Weapon, Tool, Equipment, Accessory }
    public enum ItemSubtype { None, Consumable, Tool, Weapon, Equipable, Accessory }

    [Serializable]
    [CreateAssetMenu(fileName = "Item", menuName = "URMG/Item")]
    public class ItemData : ScriptableObject
    {
        [Header("Item Attributes")]
        public AssetReference AssetReference;
        public string Id;
        public string Name;
        [TextArea] public string Description;
        public Sprite Sprite;
        public int StackSize;
        public ItemType Type;
        public ItemSubtype Subtype;
        public bool IsMaterial;
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