using System;
using System.Collections.Generic;
using System.IO;

using UnityEngine;
using UnityEngine.AddressableAssets;
using Newtonsoft.Json;

using UZSG.Systems;
using UZSG.Saves;
using UZSG.Attributes;

namespace UZSG.Data
{
    [Serializable]
    [CreateAssetMenu(fileName = "New Entity Data", menuName = "UZSG/Entity/Entity Data")]
    public class EntityData : BaseData
    {
        protected string defaultsPath = Application.dataPath + "/Resources/Defaults/Entities/";

        [Header("Entity Data")]
        public AssetReference AssetReference;
        public string Name;
        
        [Header("Base Attributes")]
        public List<Attributes.Attribute> Attributes;
        
        [Header("Audio Data")]
        public AudioAssetsData AudioAssetsData;
        
        public virtual T GetDefaultsJson<T>() where T : SaveData
        {
            var filepath = defaultsPath + $"{Id}_defaults.json";

            if (!File.Exists(filepath))
            {
                Game.Console.LogWarning($"'{Id}_defaults' not found, creating new one...");
                WriteDefaultsJson();
            }
            
            var defaultsJson = File.ReadAllText(filepath);
            return JsonConvert.DeserializeObject<T>(defaultsJson);
        }

#if UNITY_EDITOR
        public virtual void ReadDefaultsJson()
        {
            var filepath = defaultsPath + $"{Id}_defaults.json";
            var defaultsJson = File.ReadAllText(filepath);
            var defaults = JsonConvert.DeserializeObject<PlayerSaveData>(defaultsJson);

            /// Attributes
            Attributes.Clear();
            foreach (var attrSave in defaults.Attributes)
            {
                var attrData = Resources.Load<AttributeData>($"Data/Attributes/{attrSave.Id}");
                if (attrData != null)
                {
                    var newAttr = new Attributes.Attribute(attrData);
                    newAttr.ReadSaveJson(attrSave);
                    Attributes.Add(newAttr);
                }
                else
                {
                    Debug.LogWarning($"Invalid Attribute Id '{attrSave.Id}'. It does not exist.");
                }
            }
        }
#endif

        public virtual void WriteDefaultsJson()
        {
            var saveData = new EntitySaveData();
            
            /// Attributes
            var ac = new AttributeCollection();
            ac.AddList(Attributes);
            saveData.Attributes = ac.WriteSaveJson();

            WriteToFile(saveData);
        }

        protected void WriteToFile(SaveData saveData)
        {
            var filepath = defaultsPath + $"{Id}_defaults.json";
            if (!Directory.Exists(defaultsPath)) Directory.CreateDirectory(defaultsPath);///
            var defaultsJson = JsonConvert.SerializeObject(saveData, Formatting.Indented);
            File.WriteAllText(filepath, defaultsJson);
            Debug.Log($"Wrote defaults for '{Id}_defaults.json' at '{defaultsPath}'");
        }
    }
}
