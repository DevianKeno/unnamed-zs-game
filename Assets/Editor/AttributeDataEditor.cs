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
            group,
            description;

        protected override void OnEnable()
        {
            base.OnEnable();

            nameProperty = serializedObject.FindProperty("Name");
            description = serializedObject.FindProperty("Description");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            base.OnInspectorGUI();
            AttributeData attributeData = (AttributeData) target;
            
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(nameProperty);
            EditorGUILayout.PropertyField(description);
            
            serializedObject.ApplyModifiedProperties();
        }
    }
}