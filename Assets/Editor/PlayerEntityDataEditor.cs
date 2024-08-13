using UnityEditor;

using UZSG.Entities;

namespace UZSG.UnityEditor
{
    [CustomEditor(typeof(PlayerEntityData))]
    public class PlayerEntityDataEditor : EntityDataEditor
    {
        SerializedProperty knownrecipes;

        protected override void OnEnable()
        {
            base.OnEnable();
            
            knownrecipes = serializedObject.FindProperty("KnownRecipes");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            base.OnInspectorGUI();
            PlayerEntityData data = (PlayerEntityData) target;
            
            EditorGUILayout.PropertyField(knownrecipes);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
