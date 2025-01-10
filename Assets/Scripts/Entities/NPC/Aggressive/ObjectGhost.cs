using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

using UZSG.Data;
using UZSG.Interactions;
using UZSG.Items.Weapons;
using UZSG.Objects;
using UZSG.Systems;

namespace UZSG.Entities
{
    /// <summary>
    /// Ghost visual for objects when building.
    /// </summary>
    public class ObjectGhost : Entity
    {
        public enum BuildableState {
            Valid, Invalid
        }

        public static string[] UNBUILDABLE_LAYERS = { "Entities", "Objects" };

        int _collisionCount = 0;
        bool _canPlace = false;
        public bool IsVisible { get; protected set; } = false;
        public event Action<BuildableState> OnStateChanged;
        bool _hasObjectsLoaded = false;
        GameObject validGhost;
        GameObject invalidGhost;
        ObjectData objectData;
        BaseObject actualObject;
        GameObject _objectModel;
        List<Material> ghostMaterials;
        ColliderProxy _colliderProxy;
        /// <summary>
        /// int is InstanceId.
        /// </summary>
        HashSet<int> _collidedObjects;

        Material validBuildMaterial;
        Material invalidBuildMaterial;

        LayerMask unbuildableLayers;

        protected override void Start()
        {
            base.Start();

            validBuildMaterial = Resources.Load<Material>("Materials/Building Valid");
            invalidBuildMaterial = Resources.Load<Material>("Materials/Building Invalid");

            unbuildableLayers = 0;
            foreach (var layerName in UNBUILDABLE_LAYERS)
            {
                int layer = LayerMask.NameToLayer(layerName);
                if (layer != -1)
                {
                    unbuildableLayers.value |= 1 << layer;
                }
            }
            _collidedObjects = new();
        }


        #region Public methods

        /// <summary>
        /// Set the displayed object ghost.
        /// </summary>
        public void SetObject(ObjectData data)
        {
            if (data == null || !data.IsValid())
            {
                return;
            }

            objectData = data;
            Addressables.LoadAssetAsync<GameObject>(data.Object).Completed += (a) =>
            {
                if (a.Status == AsyncOperationStatus.Succeeded)
                {
                    _objectModel = a.Result;
                    CreateGhosts();
                }
            };
        }

        public void Show()
        {
            IsVisible = true;
            _canPlace = false;
            _collisionCount = 0;
            _collidedObjects = new();
            if (_hasObjectsLoaded)
            {
                invalidGhost.gameObject.SetActive(false);
                validGhost.gameObject.SetActive(false);

                var colliders = Physics.OverlapBox(transform.position, validGhost.transform.localScale / 2, transform.rotation, unbuildableLayers);
                foreach (var collider in colliders)
                {
                    var instanceID = collider.gameObject.GetInstanceID();
                    if (!_collidedObjects.Contains(instanceID))
                    {
                        _collisionCount++;
                        _collidedObjects.Add(instanceID);
                    }
                }
            }
            UpdateGhostState();
        }

        public void Hide()
        {
            IsVisible = false;
            _canPlace = false;
            _collisionCount = 0;
            _collidedObjects = new();
            if (_hasObjectsLoaded)
            {
                invalidGhost.gameObject.SetActive(false);
                validGhost.gameObject.SetActive(false);
            }
        }

        #endregion


        void CreateGhosts()
        {
            validGhost = Instantiate(_objectModel, Position, Quaternion.identity, transform);
            validGhost.name = $"{objectData.Name} (Object Ghost)";
            ApplyMaterialAll(validGhost, validBuildMaterial);            
            var colliders = validGhost.GetComponentsInChildren<Collider>();
            foreach (var c in colliders)
            {
                c.isTrigger = true;
            }
            if (validGhost.TryGetComponent(out Rigidbody rb))
            {
                rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
            }
            AddCollisionProxy(validGhost);
            
            invalidGhost = Instantiate(_objectModel, Position, Quaternion.identity, transform);
            invalidGhost.name = $"{objectData.Name} (Object Ghost)";
            ApplyMaterialAll(invalidGhost, invalidBuildMaterial);
            colliders = invalidGhost.GetComponentsInChildren<Collider>();
            foreach (var c in colliders)
            {
                c.isTrigger = true;
            }
            if (validGhost.TryGetComponent(out rb))
            {
                rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
            }
            AddCollisionProxy(invalidGhost);
            _hasObjectsLoaded = validGhost != null && invalidGhost != null;
            UpdateGhostState();
        }

        void AddCollisionProxy(GameObject obj)
        {
            _colliderProxy = obj.AddComponent<ColliderProxy>();

            _colliderProxy.OnTriggerEntered -= OnTriggerEntered;
            _colliderProxy.OnTriggerEntered += OnTriggerEntered;
            
            _colliderProxy.OnTriggerExited -= OnTriggerExited;
            _colliderProxy.OnTriggerExited += OnTriggerExited;
        }

        void OnTriggerEntered(Collider other)
        {
            if (unbuildableLayers.Includes(other.gameObject.layer))
            {
                var instanceID = other.gameObject.GetInstanceID();
                if (!_collidedObjects.Contains(instanceID))
                {
                    _collisionCount++;
                    _collidedObjects.Add(instanceID);
                    UpdateGhostState();
                }
            }
        }
        
        void OnTriggerExited(Collider other)
        {
            if (unbuildableLayers.Includes(other.gameObject.layer))
            {
                _collisionCount = Mathf.Max(0, _collisionCount - 1);
                var instanceID = other.gameObject.GetInstanceID();
                if (_collidedObjects.Contains(instanceID))
                {
                    _collidedObjects.Remove(instanceID);
                }
                UpdateGhostState();
            }
        }

        void UpdateGhostState()
        {
            if (_collisionCount <= 0)
            {
                SetBuildableState();
            }
            else
            {
                SetUnbuildableState();
            }
        }

        void SetBuildableState()
        {
            if (_hasObjectsLoaded)
            {
                invalidGhost.gameObject.SetActive(false);
                validGhost.gameObject.SetActive(true);
            }
            _canPlace = true;
        }

        void SetUnbuildableState()
        {
            if (_hasObjectsLoaded)
            {
                validGhost.gameObject.SetActive(false);
                invalidGhost.gameObject.SetActive(true);
            }
            _canPlace = false;
        }

        public bool TryPlaceObject()
        {
            if (!_canPlace)
            {
                return false; /// TODO: add prompt
            }
            
            var go = Instantiate(_objectModel, Position, Rotation);
            if (go.TryGetComponent(out actualObject))
            {
                actualObject.Place();
                return true;
            }
            else
            {
                Destroy(go);
                return false;
            }
        }
        
        void ApplyMaterialAll(GameObject gameObject, Material material)
        {
            var meshRenderers = gameObject.GetComponentsInChildren<MeshRenderer>();
            foreach (var meshRenderer in meshRenderers)
            {
                var materials = meshRenderer.materials;
                for (int i = 0; i < materials.Length; i++)
                {
                    materials[i] = material;
                }
                meshRenderer.materials = materials;
            }
        }
    }
}