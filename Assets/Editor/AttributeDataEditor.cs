using UnityEditor;

using UZSG.Data;
using UZSG.Attributes;
using UnityEngine;

namespace UZSG.UnityEditor
{
    [CustomEditor(typeof(AttributeData))]
    public class AttributeDataEditor : BaseDataEditor
    {
        SerializedProperty nameProperty,
            description,
            type,
            change,
            cycle;

        protected override void OnEnable()
        {
            base.OnEnable();

            nameProperty = serializedObject.FindProperty("Name");
            description = serializedObject.FindProperty("Description");
            type = serializedObject.FindProperty("Type");
            // change = serializedObject.FindProperty("Change");
            // cycle = serializedObject.FindProperty("Cycle");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            base.OnInspectorGUI();
            AttributeData attributeData = (AttributeData) target;
            
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(nameProperty);
            EditorGUILayout.PropertyField(description);
            EditorGUILayout.PropertyField(type);
            if (attributeData.Type == Type.Generic)
            {

            }
            else if (attributeData.Type == Type.Vital)
            {
                // EditorGUILayout.PropertyField(change);
                // EditorGUILayout.PropertyField(cycle);
            }
            
            serializedObject.ApplyModifiedProperties();
        }
    }
}