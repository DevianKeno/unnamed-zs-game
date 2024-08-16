using UnityEditor;
using UZSG.Data;

namespace UZSG.UnityEditor
{
    [CustomEditor(typeof(WildlifeData))]
    public class WildlifeDataEditor : EntityDataEditor
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
