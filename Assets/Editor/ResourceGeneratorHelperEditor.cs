using UnityEditor;
using UnityEngine;

using UZSG.Worlds;

namespace UZSG.UnityEditor
{
    [CustomEditor(typeof(ResourceGeneratorHelper))]
    public class ResourceGeneratorHelperEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            base.OnInspectorGUI();

            var generator = (ResourceGeneratorHelper) target;

            if (GUILayout.Button("Generate Instances"))
            {
                if (generator.Target == null)
                {
                    Debug.LogError("Target instance not set");
                    return;
                }
                generator.PlaceTargetInstances();
            }

            EditorUtility.SetDirty(this);
            serializedObject.ApplyModifiedProperties();
        }
    }
}