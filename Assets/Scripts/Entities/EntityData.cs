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
        public List<Attributes.Attribute> BaseAttributes;
        
        [Header("Audio Data")]
        public AudioAssetsData AudioAssetsData;
       
        /// <summary>
        /// Retrieves the default parameters set in Unity by developers for this Entity.
        /// </summary>
        public virtual T GetDefaultSaveData<T>() where T : EntitySaveData
        {
            var filepath = defaultsPath + $"{Id}_defaults.json";

            if (!File.Exists(filepath))
            {
                Game.Console.LogWarn($"'{Id}_defaults' not found, creating new one...");
                WriteDefaultsJson();
            }

            var json = File.ReadAllText(filepath);
            return JsonConvert.DeserializeObject<T>(json);
        }

#if UNITY_EDITOR
        /// <summary>
        /// 
        /// </summary>
        public virtual void ReadDefaultsJson()
        {
            var filepath = defaultsPath + $"{Id}_defaults.json";
            var defaultsJson = File.ReadAllText(filepath);
            var defaults = JsonConvert.DeserializeObject<EntitySaveData>(defaultsJson);

            /// Attributes
            BaseAttributes.Clear();
            foreach (var attrSave in defaults.Attributes)
            {
                var attrData = Resources.Load<AttributeData>($"Data/Attributes/{attrSave.Id}");
                if (attrData != null)
                {
                    var newAttr = new Attributes.Attribute(attrData);
                    newAttr.ReadSaveData(attrSave);
                    BaseAttributes.Add(newAttr);
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
            ac.AddList(BaseAttributes);
            saveData.Attributes = ac.WriteSaveData();

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
