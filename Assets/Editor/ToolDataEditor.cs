using UnityEditor;
using UnityEngine;
using UZSG.Data;

namespace UZSG.UnityEditor
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(ToolData))]
    public class ToolDataEditor : ItemDataEditor
    {
        protected override void OnEnable()
        {
            base.OnEnable();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            base.OnInspectorGUI();
            ToolData toolData = (ToolData) target;
            
            serializedObject.ApplyModifiedProperties();
        }
    }
}