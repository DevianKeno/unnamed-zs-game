using System;

using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;

using UZSG.Data;
using UZSG.Entities;

namespace UZSG.UnityEditor
{
    [CustomEditor(typeof(PlayerEntityData))]
    public class PlayerEntityDataEditor : EntityDataEditor
    {
        SerializedProperty vital,
            generic,
            knownrecipes;

        protected override void OnEnable()
        {
            base.OnEnable();
            
            vital = serializedObject.FindProperty("Vitals");
            generic = serializedObject.FindProperty("Generic");
            knownrecipes = serializedObject.FindProperty("KnownRecipes");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            base.OnInspectorGUI();
            
            EditorGUILayout.PropertyField(vital);
            EditorGUILayout.PropertyField(generic);
            EditorGUILayout.PropertyField(knownrecipes);
            
            serializedObject.ApplyModifiedProperties();
        }
    }
}
