using UnityEditor;
using UnityEngine;

using UZSG.Entities;

namespace UZSG.UnityEditor
{
    [CustomEditor(typeof(PlayerEntityData))]
    public class PlayerEntityDataEditor : EntityDataEditor
    {
        SerializedProperty attributes,
            knownrecipes;

        protected override void OnEnable()
        {
            base.OnEnable();
            
            attributes = serializedObject.FindProperty("Attributes");
            knownrecipes = serializedObject.FindProperty("KnownRecipes");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            base.OnInspectorGUI();
            PlayerEntityData data = (PlayerEntityData) target;
            
            EditorGUILayout.PropertyField(attributes);
            EditorGUILayout.PropertyField(knownrecipes);
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

            serializedObject.ApplyModifiedProperties();
        }
    }
}
