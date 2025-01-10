using UnityEditor;

using UZSG.Data;

namespace UZSG.UnityEditor
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(BaseData))]
    public class BaseDataEditor : Editor
    {
        SerializedProperty sourceId,
            id,
            tags;
        
        protected virtual void OnEnable()
        {
            sourceId = serializedObject.FindProperty("Namespace");
            id = serializedObject.FindProperty("Id");
            tags = serializedObject.FindProperty("Tags");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(sourceId);
            EditorGUILayout.PropertyField(id);
            EditorGUILayout.PropertyField(tags);

            serializedObject.ApplyModifiedProperties();
        }
    }
}