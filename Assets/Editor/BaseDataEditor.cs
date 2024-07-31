using UnityEditor;

namespace UZSG.UnityEditor
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(BaseData))]
    public class BaseDataEditor : Editor
    {
        SerializedProperty id;

        protected virtual void OnEnable()
        {
            id = serializedObject.FindProperty("Id");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(id);

            serializedObject.ApplyModifiedProperties();
        }
    }
}