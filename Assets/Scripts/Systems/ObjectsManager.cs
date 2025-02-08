using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

using UZSG.Data;
using UZSG.Objects;

namespace UZSG
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
            Game.Console.LogInfo("Reading data: Objects...");
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

        /// <summary>
        /// Place a new Object at position.
        /// Should only be called when within a world.
        /// </summary>
        public async void PlaceNew(string objectId, Vector3 position = default, OnObjectPlaceCompleted callback = null)
        {
            if (!_objectsDict.ContainsKey(objectId))
            {
                Game.Console.LogDebug($"Object '{objectId}' does not exist!");
                return;
            }

            try
            {
                var objData = _objectsDict[objectId];
                var asyncOp = Addressables.LoadAssetAsync<GameObject>(objData.Object);
                await asyncOp.Task;

                if (asyncOp.Status == AsyncOperationStatus.Succeeded)
                {
                    var go = Instantiate(asyncOp.Result, position, Quaternion.identity/*, Game.World.CurrentWorld.objectsContainer*/);
                    go.name = $"{objData.DisplayName} (Object)";
                    if (go.TryGetComponent(out BaseObject baseObject))
                    {
                        var info = new ObjectPlacedInfo()
                        {
                            Object = baseObject
                        };
                        callback?.Invoke(info);
                        baseObject.PlaceInternal();
                        // entity.OnSpawnInternal();
                        // OnEntitySpawned?.Invoke(new()
                        // {
                        //     Entity = entity
                        // });
                        
                        // Game.Console.LogDebug($"Placed object '{objectId}' at ({position.x}, {position.y}, {position.z})");
                        Addressables.Release(asyncOp);
                        return;
                    }
                    Destroy(go);
                }

                Game.Console.LogDebug($"Tried to place Object '{objectId}', but failed miserably");
                
            }
            catch (Exception ex)
            {
                Game.Console.LogError($"An internal error occured when trying to place object {objectId}.");
                Debug.LogException(ex);
            }
        }
        
        public delegate void OnObjectPlaceCompleted<T>(ObjectPlacedInfo<T> info);
        public struct ObjectPlacedInfo<T>
        {
            public T Object { get; set; }
        }

        /// <summary>
        /// Place a new Object at position.
        /// Should only be called when within a world.
        /// </summary>
        public async void PlaceNew<T>(string objectId, Vector3 position = default, OnObjectPlaceCompleted<T> callback = null) where T : BaseObject
        {
            if (!_objectsDict.ContainsKey(objectId))
            {
                Game.Console.LogDebug($"Object '{objectId}' does not exist!");
                return;
            }
            
            try
            {
                var objData = _objectsDict[objectId];
                var asyncOp = Addressables.LoadAssetAsync<GameObject>(objData.Object);
                await asyncOp.Task;
                
                if (asyncOp.Status == AsyncOperationStatus.Succeeded)
                {
                    var go = Instantiate(asyncOp.Result, position, Quaternion.identity/*, Game.World.CurrentWorld.objectsContainer*/);
                    go.name = $"{objData.DisplayName} (Object)";
                    if (go.TryGetComponent(out BaseObject baseObject))
                    {
                        var info = new ObjectPlacedInfo<T>()
                        {
                            Object = baseObject as T
                        };
                        callback?.Invoke(info);
                        // entity.OnSpawnInternal();
                        // OnEntitySpawned?.Invoke(new()
                        // {
                        //     Entity = entity
                        // });

                        // Game.Console.LogDebug($"Placed object '{objectId}' at ({position.x}, {position.y}, {position.z})");
                        Addressables.Release(asyncOp);
                        return;
                    }
                    Destroy(go);
                }

                Game.Console.LogDebug($"Tried to place Object '{objectId}', but failed miserably");
            }
            catch (Exception ex)
            {
                Game.Console.LogError($"An internal error occured when trying to place object {objectId}.");
                Debug.LogException(ex);
            }
        }
        
        /// <summary>
        /// Loads and caches the model.
        /// </summary>
        public bool LoadObjectModel(string id)
        {
            if (_cachedObjectModels.ContainsKey(id))
            {
                /// lmao idk about this
                Game.Console.LogWarn($"Object '{id}' is already loaded in memory.");
                return true;
            }
            if (!_objectsDict.TryGetValue(id, out var objData))
            {
                Game.Console.LogInfo($"Failed to load Object '{id}' as it does not exists.");
                return false;
            }
            if (objData.Object == null)
            {
                Game.Console.LogWarn($"There is no Addressable Asset assigned to Object '{id}'.");
                return false;
            }

            /// Load model
            Addressables.LoadAssetAsync<GameObject>(objData.Object).Completed += (a) =>
            {
                if (a.Status == AsyncOperationStatus.Succeeded)
                {
                    _cachedObjectModels[id] = a.Result;
                    // OnDoneLoadModel?.Invoke(this, objData.Id);
                }
            };
            
            return true;
        }
        
        public ObjectData GetData(string id)
        {
            if (_objectsDict.ContainsKey(id))
            {
                return _objectsDict[id];
            }

            Game.Console.LogInfo("Invalid Object Id");
            return null;
        }
        
        public bool TryGetData(string id, out ObjectData objectData)
        {
            if (_objectsDict.ContainsKey(id))
            {
                objectData = _objectsDict[id];
                return true;
            }
            
            Game.Console.LogInfo("Invalid Object Id");
            objectData = null;
            return false;
        }
    }
}