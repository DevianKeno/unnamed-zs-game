using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UZSG.Attributes;

namespace UZSG.Entities
{
    [Serializable]
    [CreateAssetMenu(fileName = "EntityData", menuName = "URMG/Entity Data/Entity Data")]
    public class EntityData : ScriptableObject
    {
        public AssetReference AssetReference;
        public string Id;
        public string Name;
    }
    
    // [CustomEditor(typeof(EntityData))]
    // public class EntityDataEditor : Editor
    // {
    //     SerializedProperty attributable;
    //     SerializedProperty attributes;

    //     void OnEnable()
    //     {
    //         attributable = serializedObject.FindProperty("Attributable");
    //         attributes = serializedObject.FindProperty("Attributes");
    //     }

    //     public override void OnInspectorGUI()
    //     {
    //         base.OnInspectorGUI();
    //         EntityData entityData = (EntityData)target;
            
    //         if (entityData.Attributable)
    //         {
    //             EditorGUILayout.PropertyField(attributes);
    //         }
            
    //         serializedObject.Update();
    //         serializedObject.ApplyModifiedProperties();
    //     }
    // }
}
