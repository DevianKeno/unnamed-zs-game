using System;
using UnityEditor;
using UnityEngine;

namespace UZSG.Attributes
{
    public enum Type { Generic, Vital }
    public enum Change { Static, Regen, Degen }
    public enum Cycle { PerSecond, PerTick }

    [Serializable]
    [CreateAssetMenu(fileName = "Attribute", menuName = "UZSG/Attribute")]
    public class AttributeData : ScriptableObject
    {
        public Type Type;
        public string Id;
        public string Name;
        [TextArea] public string Description;
        [HideInInspector] public Change Change;
        [HideInInspector] public Cycle Cycle;
    }

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