using UnityEditor;
using UnityEngine;

using UZSG.Data;

namespace UZSG.UnityEditor
{
    [CustomEditor(typeof(EntityData))]
    public class EntityDataEditor : BaseDataEditor
    {
        SerializedProperty assetReference,
            displayNameProperty,
            baseAttributes,
            audioAssetsData;

        protected override void OnEnable()
        {
            base.OnEnable();
            
            assetReference = serializedObject.FindProperty("AssetReference");
            displayNameProperty = serializedObject.FindProperty("DisplayName");
            baseAttributes = serializedObject.FindProperty("BaseAttributes");
            audioAssetsData = serializedObject.FindProperty("AudioAssetsData");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            base.OnInspectorGUI();
            EntityData data = (EntityData) target;
            
            EditorGUILayout.PropertyField(assetReference);
            EditorGUILayout.PropertyField(displayNameProperty);
            EditorGUILayout.PropertyField(baseAttributes);
            
            if (GUILayout.Button("Save to Defaults Json"))
            {
                data.WriteDefaultsJson();
                EditorUtility.SetDirty(data);
            }
            if (GUILayout.Button("Read to Defaults Json"))
            {
                data.ReadDefaultsJson();
                EditorUtility.SetDirty(data);
            }

            EditorGUILayout.PropertyField(audioAssetsData);
            
            serializedObject.ApplyModifiedProperties();
        }
    }
}
