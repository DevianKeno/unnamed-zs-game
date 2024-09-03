using UnityEditor;
using UnityEngine;
using UZSG.Entities;

namespace UZSG.UnityEditor
{
    [CustomEditor(typeof(EntityHitboxController))]
    public class EntityHitboxControllerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            serializedObject.Update();
            var ehc = (EntityHitboxController) target;

            if (GUILayout.Button("Reinitialize Hitboxes"))
            {
                ehc.ReinitializeHitboxes();
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}