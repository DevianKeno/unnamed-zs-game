using UnityEditor;

using UZSG.Data;
using UZSG.Attributes;
using UnityEngine;

namespace UZSG.UnityEditor
{
    [CustomEditor(typeof(AttributeData))]
    public class AttributeDataEditor : BaseDataEditor
    {
        SerializedProperty displayNameProperty,
            group,
            description;

        protected override void OnEnable()
        {
            base.OnEnable();

            displayNameProperty = serializedObject.FindProperty("DisplayName");
            description = serializedObject.FindProperty("Description");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            base.OnInspectorGUI();
            AttributeData attributeData = (AttributeData) target;
            
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(displayNameProperty);
            EditorGUILayout.PropertyField(description);
            
            serializedObject.ApplyModifiedProperties();
        }
    }
}