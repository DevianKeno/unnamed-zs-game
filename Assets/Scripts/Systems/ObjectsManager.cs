using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

using UZSG.Data;
using UZSG.Items;
using UZSG.Objects;

namespace UZSG.Systems
{
    public class ObjectsManager : MonoBehaviour
    {
        bool _isInitialized;
        public bool IsInitialized => _isInitialized;
        Dictionary<string, ObjectData> _objectsDict = new();
        Dictionary<string, GameObject> _cachedObjectModels = new();

        public event Action<ObjectPlacedInfo> OnObjectPlaced;

        internal void Initialize()
        {
            if (_isInitialized) return;
            _isInitialized = true;
            
            var startTime = Time.time;
            Game.Console.Log("Reading data: Objects...");
            foreach (var obj in Resources.LoadAll<ObjectData>("Data/Objects"))
            {
                _objectsDict[obj.Id] = obj;
            }
        }

        public delegate void OnObjectPlaceCompleted(ObjectPlacedInfo info);
        public struct ObjectPlacedInfo
        {
            public BaseObject Object { get; set; }
        }

        public void Place(string objectId, Vector3 position = default, OnObjectPlaceCompleted callback = null)
        {
            if (!_objectsDict.ContainsKey(objectId))
            {
                Game.Console.Debug($"Entity '{objectId}' does not exist!");
                return;
            }

            var objData = _objectsDict[objectId];
            Addressables.LoadAssetAsync<GameObject>(objData.Model).Completed += (a) =>
            {
                if (a.Status == AsyncOperationStatus.Succeeded)
                {
                    var go = Instantiate(a.Result, position, Quaternion.identity, transform);
                    go.name = $"{objData.Name} (Object)";
                    if (go.TryGetComponent(out BaseObject entity))
                    {
                        var info = new ObjectPlacedInfo()
                        {
                            Object = entity
                        };
                        callback?.Invoke(info);
                        // entity.OnSpawnInternal();
                        // OnEntitySpawned?.Invoke(new()
                        // {
                        //     Entity = entity
                        // });
                        
                        Game.Console.Log($"Placed object '{objectId}' at ({position.x}, {position.y}, {position.z})");
                        return;
                    }
                    Destroy(go);
                }

                Game.Console.Debug($"Tried to place Object '{objectId}', but failed miserably");
            };
        }
        
        public delegate void OnObjectPlaceCompleted<T>(ObjectPlacedInfo<T> info);
        public struct ObjectPlacedInfo<T>
        {
            public T Object { get; set; }
        }

        public void Place<T>(string objectId, Vector3 position = default, OnObjectPlaceCompleted<T> callback = null) where T : BaseObject
        {
            if (!_objectsDict.ContainsKey(objectId))
            {
                Game.Console.Debug($"Entity '{objectId}' does not exist!");
                return;
            }

            var objData = _objectsDict[objectId];
            Addressables.LoadAssetAsync<GameObject>(objData.Model).Completed += (a) =>
            {
                if (a.Status == AsyncOperationStatus.Succeeded)
                {
                    var go = Instantiate(a.Result, position, Quaternion.identity, transform);
                    go.name = $"{objData.Name} (Object)";
                    if (go.TryGetComponent(out BaseObject entity))
                    {
                        var info = new ObjectPlacedInfo<T>()
                        {
                            Object = entity as T
                        };
                        callback?.Invoke(info);
                        // entity.OnSpawnInternal();
                        // OnEntitySpawned?.Invoke(new()
                        // {
                        //     Entity = entity
                        // });
                        
                        Game.Console.Log($"Placed object '{objectId}' at ({position.x}, {position.y}, {position.z})");
                        return;
                    }
                    Destroy(go);
                }

                Game.Console.Debug($"Tried to place Object '{objectId}', but failed miserably");
            };
        }
        
        /// <summary>
        /// Loads and caches the model.
        /// </summary>
        public bool LoadObjectModel(string id)
        {
            if (_cachedObjectModels.ContainsKey(id))
            {
                /// lmao idk about this
                Game.Console.Warn($"Object '{id}' is already loaded in memory.");
                return true;
            }

            if (_objectsDict.ContainsKey(id))
            {
                var objData = _objectsDict[id];

                if (objData.Model != null)
                {
                    /// Load model
                    Addressables.LoadAssetAsync<GameObject>(objData.Model).Completed += (a) =>
                    {
                        if (a.Status == AsyncOperationStatus.Succeeded)
                        {
                            _cachedObjectModels[id] = a.Result;
                            // OnDoneLoadModel?.Invoke(this, objData.Id);
                        }
                    };
                    
                    return true;
                }
                else
                {
                    Game.Console.Warn($"There is no Addressable Asset assigned to Object '{id}'.");
                    return false;
                }
            }
            else
            {
                Game.Console.Log($"Failed to load Object '{id}' as it does not exists.");
                return false;
            }            
        }
        
        public ObjectData GetData(string id)
        {
            if (_objectsDict.ContainsKey(id))
            {
                return _objectsDict[id];
            }

            Game.Console.Log("Invalid Object Id");
            return null;
        }
        
        public bool TryGetData(string id, out ObjectData itemData)
        {
            if (_objectsDict.ContainsKey(id))
            {
                itemData = _objectsDict[id];
                return true;
            }
            
            Game.Console.Log("Invalid Object Id");
            itemData = null;
            return false;
        }
    }
}