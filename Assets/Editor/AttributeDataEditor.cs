using UnityEditor;

using UZSG.Data;
using UZSG.Attributes;

namespace UZSG.UnityEditor
{
    [CustomEditor(typeof(AttributeData))]
    public class AttributeDataEditor : Editor
    {
        SerializedProperty type;
        SerializedProperty change;
        SerializedProperty cycle;

        void OnEnable()
        {
            type = serializedObject.FindProperty("Type");
            change = serializedObject.FindProperty("Change");
            cycle = serializedObject.FindProperty("Cycle");
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            serializedObject.Update();
            AttributeData attributeData = (AttributeData)target;
            
            if (attributeData.Type == Type.Generic)
            {

            } else if (attributeData.Type == Type.Vital)
            {
                EditorGUILayout.PropertyField(change);
                EditorGUILayout.PropertyField(cycle);
            }
            
            serializedObject.ApplyModifiedProperties();
        }
    }
}