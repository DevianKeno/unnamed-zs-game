using UnityEditor;
using UnityEngine;

using UZSG.Data;
using UZSG.Worlds;

namespace UZSG.UnityEditor
{
    [CustomEditor(typeof(TerrainTextureHelper))]
    public class TerrainTextureHelperEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            base.OnInspectorGUI();
            var helper = (TerrainTextureHelper) target;
            
            if (GUILayout.Button("Collect Terrains"))
            {
                helper.CollectTerrains();
            }

            EditorGUILayout.Space();
            if (GUILayout.Button("Apply"))
            {
                helper.ApplyTexturesToAll();
            }
        }
    }
}