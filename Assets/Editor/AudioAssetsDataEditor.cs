using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UZSG.Data;

namespace UZSG.UnityEditor
{
    [CustomEditor(typeof(AudioAssetsData))]
    public class AudioAssetsDataEditor : Editor
    {
        SerializedProperty path,
            audioClips;
        
        protected virtual void OnEnable()
        {
            path = serializedObject.FindProperty("Path");
            audioClips = serializedObject.FindProperty("AudioClips");
        }
        
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            AudioAssetsData audioAssetsData = (AudioAssetsData) target;

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Audio Data", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(path);
            if (GUILayout.Button("Load Audio Assets"))
            {
                PopulateAssetReferences(audioAssetsData);
            }
            EditorGUILayout.PropertyField(audioClips);

            serializedObject.ApplyModifiedProperties();
        }
    
#if UNITY_EDITOR
        void PopulateAssetReferences(AudioAssetsData audioAssetsData)
        {
            if (string.IsNullOrEmpty(audioAssetsData.Path))
            {
                audioAssetsData.Path = AssetDatabase.GUIDToAssetPath(AssetDatabase.GetAssetPath(audioAssetsData));
                return;
            }

            var settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings == null)
            {
                Debug.LogError("Addressable Asset Settings not found.");
                return;
            }

            string[] guids = AssetDatabase.FindAssets("t:AudioClip", new[] { audioAssetsData.Path });
            
            audioAssetsData.AudioClips.Clear();
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var assetReference = new AssetReference(AssetDatabase.AssetPathToGUID(path));
                audioAssetsData.AudioClips.Add(assetReference);
            }
            
            EditorUtility.SetDirty(audioAssetsData);
        }
#endif
    }
}