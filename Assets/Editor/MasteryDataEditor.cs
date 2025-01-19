using UnityEditor;

using UZSG.Masteries;

namespace UZSG.UnityEditor
{
    [CustomEditor(typeof(MasteryData))]
    public class MasteryDataEditor : Editor
    {
        SerializedProperty isLeveled;
        SerializedProperty minLevel;
        SerializedProperty maxLevel;

        void OnEnable()
        {
            isLeveled = serializedObject.FindProperty("IsLeveled");
            minLevel = serializedObject.FindProperty("MinLevel");
            maxLevel = serializedObject.FindProperty("MaxLevel");
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            serializedObject.Update();
            MasteryData attributeData = (MasteryData)target;
            
            if (attributeData.IsLeveled == true)
            {
                EditorGUILayout.PropertyField(minLevel);
                EditorGUILayout.PropertyField(maxLevel);
            }
            
            serializedObject.ApplyModifiedProperties();
        }
    }
}
