using System;
using UnityEditor;
using UnityEngine;

namespace UZSG.Masteries
{
    [Serializable]
    [CreateAssetMenu(fileName = "Mastery Data", menuName = "URMG/Mastery Data")]
    public class MasteryData : ScriptableObject
    {
        public string Id;
        public string Name;
        public string Description;
        public bool IsLeveled;
        [HideInInspector] public int MinLevel;
        [HideInInspector] public int MaxLevel;
        public bool IsFinal;
    }

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
