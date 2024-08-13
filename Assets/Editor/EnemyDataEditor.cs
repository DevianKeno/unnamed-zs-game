using UnityEditor;
using UZSG.Data;

namespace UZSG.UnityEditor
{
    [CustomEditor(typeof(EnemyData))]
    public class EnemyDataEditor : EntityDataEditor
    {
        protected override void OnEnable()
        {
            base.OnEnable();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            base.OnInspectorGUI();
            
            serializedObject.ApplyModifiedProperties();
        }
    }
}
