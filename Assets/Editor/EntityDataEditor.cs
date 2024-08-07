using System;

using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;

using UZSG.Data;
using UZSG.Entities;

namespace UZSG.UnityEditor
{
    [CustomEditor(typeof(EntityData))]
    public class EntityDataEditor : BaseDataEditor
    {
        SerializedProperty assetReference,
            nameProperty,
            audioAssetsData;

        protected override void OnEnable()
        {
            base.OnEnable();
            
            assetReference = serializedObject.FindProperty("AssetReference");
            nameProperty = serializedObject.FindProperty("Name");
            audioAssetsData = serializedObject.FindProperty("AudioAssetsData");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            base.OnInspectorGUI();
            EntityData entityData = (EntityData) target;
            
            EditorGUILayout.PropertyField(assetReference);
            EditorGUILayout.PropertyField(nameProperty);
            EditorGUILayout.PropertyField(audioAssetsData);
            
            serializedObject.ApplyModifiedProperties();
        }
    }
}
