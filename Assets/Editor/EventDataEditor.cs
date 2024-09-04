using UnityEditor;
using UnityEngine;
using UZSG.Data;
using UZSG.Worlds.Events;

namespace UZSG.UnityEditor
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(WorldEventData))]
    public class EventDataEditor : Editor
    {   
        SerializedProperty Type;
        SerializedProperty Enabled;
        SerializedProperty AllowMultipleEvents;
        SerializedProperty ChanceToOccur;
        SerializedProperty OccurEverySecond;

        SerializedProperty WeatherTypes;
        SerializedProperty RaidTypes;

        protected virtual void OnEnable()
        {
            Type = serializedObject.FindProperty("Type");
            WeatherTypes = serializedObject.FindProperty("WeatherTypes");
            RaidTypes = serializedObject.FindProperty("RaidTypes");
            Enabled = serializedObject.FindProperty("Enabled");
            AllowMultipleEvents = serializedObject.FindProperty("AllowMultipleEvents");
            ChanceToOccur = serializedObject.FindProperty("ChanceToOccur");
            OccurEverySecond = serializedObject.FindProperty("OccurEverySecond");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            
            WorldEventData eventData = (WorldEventData) target;
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(Type);
            WorldEventType worldEventType = (WorldEventType) Type.enumValueIndex;
            EditorGUILayout.PropertyField(Enabled);
            EditorGUILayout.PropertyField(AllowMultipleEvents);
            EditorGUILayout.PropertyField(ChanceToOccur);
            EditorGUILayout.PropertyField(OccurEverySecond);

            if (worldEventType == WorldEventType.Weather)
            {
                EditorGUILayout.PropertyField(WeatherTypes);

            }
            else if (worldEventType == WorldEventType.Raid)
            {
                EditorGUILayout.PropertyField(RaidTypes);
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}