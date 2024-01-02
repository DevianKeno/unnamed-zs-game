using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace UZSG.Attributes
{
    public enum Change { Static, Regen, Degen }
    public enum Cycle { PerSecond, PerTick }

    [Serializable]
    [CreateAssetMenu(fileName = "Attribute", menuName = "URMG/Attributes/Attribute")]
    public class AttributeData : ScriptableObject
    {
        public string Id;
        public string Name;
        [TextArea] public string Description;
        public float Minimum = 0;
        /// <summary>
        /// Anything below 1 means no limit.
        /// </summary>
        public float BaseValue = 100;
        public float Multiplier = 1;
        public bool IsVital;
        public bool AllowChange;
        [HideInInspector] public Change Type;
        [HideInInspector] public Cycle Cycle;
        /// <summary>
        /// Refers to how much value is changed per cycle.
        /// </summary>
        [HideInInspector] public float ChangeAmount;
        [HideInInspector] public float ChangeMultiplier = 1;
    }

    [CustomEditor(typeof(AttributeData))]
    public class MyScriptableObjectEditor : Editor
    {
        SerializedProperty allowChange;
        SerializedProperty type;
        SerializedProperty cycle;
        SerializedProperty changeAmount;
        SerializedProperty changeMultiplier;

        void OnEnable()
        {
            allowChange = serializedObject.FindProperty("AllowChange");
            type = serializedObject.FindProperty("Type");
            cycle = serializedObject.FindProperty("Cycle");
            changeAmount = serializedObject.FindProperty("ChangeAmount");
            changeMultiplier = serializedObject.FindProperty("ChangeMultiplier");
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            serializedObject.Update();
            AttributeData attributeData = (AttributeData)target;
            
            if (attributeData.AllowChange == true)
            {
                EditorGUILayout.PropertyField(type);
                EditorGUILayout.PropertyField(cycle);
                EditorGUILayout.PropertyField(changeAmount);
                EditorGUILayout.PropertyField(changeMultiplier);
            }
            
            serializedObject.ApplyModifiedProperties();
        }
    }
}