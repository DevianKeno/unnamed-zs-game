using UnityEditor;
using UnityEngine;
using UZSG.Data;

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
            stackSize,
            type,
            subType,
            isMaterial,
            isCraftable,
            recipes,
            audioAssetsData,
            weight,
            sourceDesc;

        protected override void OnEnable()
        {
            base.OnEnable();

            assetReference = serializedObject.FindProperty("Model");
            nameProperty = serializedObject.FindProperty("Name");
            description = serializedObject.FindProperty("Description");
            sprite = serializedObject.FindProperty("Sprite");
            stackSize = serializedObject.FindProperty("StackSize");
            type = serializedObject.FindProperty("Type");
            subType = serializedObject.FindProperty("Subtype");
            isMaterial = serializedObject.FindProperty("IsMaterial");
            isCraftable = serializedObject.FindProperty("IsCraftable");
            recipes = serializedObject.FindProperty("Recipes");
            weight = serializedObject.FindProperty("Weight");
            sourceDesc = serializedObject.FindProperty("SourceDescription");
            audioAssetsData = serializedObject.FindProperty("AudioAssetsData");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            base.OnInspectorGUI();
            ItemData itemData = (ItemData) target;
            
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(nameProperty);
            EditorGUILayout.PropertyField(description);
            EditorGUILayout.PropertyField(assetReference);
            EditorGUILayout.PropertyField(sprite);
            EditorGUILayout.PropertyField(type);
            EditorGUILayout.PropertyField(subType);
            EditorGUILayout.PropertyField(weight);
            EditorGUILayout.PropertyField(stackSize);
            EditorGUILayout.PropertyField(sourceDesc);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Crafting", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(isMaterial);
            EditorGUILayout.PropertyField(isCraftable);
            if (GUILayout.Button("Get Recipes"))
            {
                GetRecipes(itemData);
            }
            EditorGUILayout.PropertyField(recipes);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Audio Data", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(audioAssetsData);

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

#if UNITY_EDITOR
        void GetRecipes(ItemData itemData)
        {
            itemData.Recipes.Clear();
            foreach (var recipeData in Resources.LoadAll<RecipeData>("Data/Recipes"))
            {
                var str = recipeData.name.Split('-');
                if (str[0] != itemData.Id) continue;
                itemData.Recipes.Add(recipeData);
            }

            EditorUtility.SetDirty(itemData);
        }
#endif
    }
}