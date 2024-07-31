using UnityEngine;
using UnityEditor;
using UZSG.Items;

namespace UZSG.UnityEditor
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(ItemData))]
    public class ItemDataEditor : BaseDataEditor
    {
        SerializedProperty assetReference,
            nameProperty,
            description,
            sprite,
            isStackable,
            stackSize,
            type,
            subType,
            isMaterial,
            isCraftable,
            recipes,
            weight;

        protected override void OnEnable()
        {
            base.OnEnable();

            assetReference = serializedObject.FindProperty("Model");
            nameProperty = serializedObject.FindProperty("Name");
            description = serializedObject.FindProperty("Description");
            sprite = serializedObject.FindProperty("Sprite");
            stackSize = serializedObject.FindProperty("StackSize");
            isStackable = serializedObject.FindProperty("IsStackable");
            type = serializedObject.FindProperty("Type");
            subType = serializedObject.FindProperty("Subtype");
            isMaterial = serializedObject.FindProperty("IsMaterial");
            isCraftable = serializedObject.FindProperty("IsCraftable");
            recipes = serializedObject.FindProperty("Recipes");
            weight = serializedObject.FindProperty("Weight");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            base.OnInspectorGUI();
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Item Attributes", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(nameProperty);
            EditorGUILayout.PropertyField(description);
            EditorGUILayout.PropertyField(assetReference);
            EditorGUILayout.PropertyField(sprite);
            EditorGUILayout.PropertyField(type);
            EditorGUILayout.PropertyField(subType);
            EditorGUILayout.PropertyField(weight);
            EditorGUILayout.PropertyField(isStackable);
            if (isStackable.boolValue)
            {
                EditorGUILayout.PropertyField(stackSize);
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Crafting", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(isMaterial);
            EditorGUILayout.PropertyField(isCraftable);

            if (isCraftable.boolValue)
            {
                EditorGUILayout.PropertyField(recipes);
            }

            // ItemType itemType = (ItemType) type.enumValueIndex;
            // if (itemType == ItemType.Weapon || itemType == ItemType.Tool)
            // {
            // }

            // ItemSubtype itemSubtype = (ItemSubtype)subType.enumValueIndex;
            // if (itemSubtype == ItemSubtype.Consumable || itemSubtype == ItemSubtype.Food)
            // {
                
            // }

            serializedObject.ApplyModifiedProperties();
        }
    }
}