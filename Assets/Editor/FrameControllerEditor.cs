using UnityEngine;
using UnityEditor;

using UZSG.UI;

namespace UZSG.UnityEditor
{
    [CustomEditor(typeof(FrameController))]
    public class FrameControllerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            var fc = target as FrameController;
            base.OnInspectorGUI();

            if (GUILayout.Button("Switch to Frame"))
            {
                fc.SwitchFrameEditor();
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}