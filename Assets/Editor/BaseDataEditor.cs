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
            sourceId = serializedObject.FindProperty("namespace");
            id = serializedObject.FindProperty("id");
            tags = serializedObject.FindProperty("tags");
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