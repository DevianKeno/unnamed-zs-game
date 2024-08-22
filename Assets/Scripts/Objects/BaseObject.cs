using System;

using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

using UZSG.Systems;
using UZSG.Data;
using UZSG.Attributes;
using UZSG.UI.Objects;
using UZSG.Interactions;
using UZSG.Saves;
using UZSG.Worlds;

namespace UZSG.Objects
{
    public abstract class BaseObject : MonoBehaviour, IAttributable, IPlaceable, IPickupable, ICollisionTarget
    {
        [SerializeField] protected ObjectData objectData;
        public ObjectData ObjectData => objectData;

        [SerializeField] protected AttributeCollection attributes;
        public AttributeCollection Attributes => attributes;

        // [SerializeField] protected AudioSourceController audioController;
        // public AudioSourceController Audio => audioController;
        
        [SerializeField] protected Animator animator;
        public Animator Animator => animator;

        protected bool isDirty;
        public bool IsDirty => isDirty;
        
        /// <summary>
        /// The transform position of this Object. 
        /// </summary>
        public Vector3 Position
        {
            get { return transform.position; }
            set { transform.position = value; }
        }
        /// <summary>
        /// The transform rotation of this Object. 
        /// </summary>
        public Quaternion Rotation
        {
            get { return transform.rotation; }
            set { transform.rotation = value; }
        }
        
        public event EventHandler<HitboxCollisionInfo> OnHit;


        #region Initializing methods

        protected virtual void Start()
        {
            if (objectData.HasAudio)
            {
                Game.Audio.LoadAudioAssets(objectData.AudioAssetsData);
            }
        }
        
        protected virtual void LoadAudioAssets()
        {
            // audioController ??= gameObject.AddComponent<AudioSourceController>();
            // audioController.LoadAudioAssetsData(objectData.AudioAssetsData);
        }

        protected virtual void LoadGUIAsset(AssetReference guiAsset, Action<ObjectGUI> onLoadCompleted = null)
        {
            if (!guiAsset.IsSet())
            {
                Game.Console.LogAndUnityLog($"There's no GUI set for Workstation '{objectData.Id}'. This won't be usable unless otherwise you set its GUI.");
                return;
            }

            Addressables.LoadAssetAsync<GameObject>(guiAsset).Completed += (a) =>
            {
                if (a.Status == AsyncOperationStatus.Succeeded)
                {
                    var go = Instantiate(a.Result);
                    
                    if (go.TryGetComponent<ObjectGUI>(out var gui))
                    {
                        onLoadCompleted?.Invoke(gui);
                        return;
                    }
                }
            };
        }

        public void ReadSaveData(ObjectSaveData saveData)
        {
            InitializeTransform(saveData.Transform);
        }

        public virtual ObjectSaveData WriteSaveData()
        {
            var saveData = new ObjectSaveData()
            {
                InstanceId = GetInstanceID(),
                Id = objectData.Id,
                Transform = new()
                {
                    Position = new System.Numerics.Vector3(
                        transform.position.x,
                        transform.position.y,
                        transform.position.z
                    ),
                    Rotation = new System.Numerics.Quaternion(
                        transform.rotation.x,
                        transform.rotation.y,
                        transform.rotation.z,
                        transform.rotation.w
                    ),
                    LocalScale = new System.Numerics.Vector3(
                        transform.localScale.x,
                        transform.localScale.y,
                        transform.localScale.z
                    )
                }
            };

            return saveData;
        }

        #endregion


        public virtual void HitBy(HitboxCollisionInfo info)
        {
            
        }

        protected virtual void Destroy()
        {
            
        }

        void InitializeTransform(TransformSaveData data)
        {
            var position = new Vector3(
                data.Position.X,
                data.Position.Y,
                data.Position.Z
            );
            var rotation = new Quaternion(
                data.Rotation.X,
                data.Rotation.Y,
                data.Rotation.Z,
                data.Rotation.W
            );
            var scale = new Vector3(
                data.LocalScale.X,
                data.LocalScale.Y,
                data.LocalScale.Z
            );
            transform.SetPositionAndRotation(position, rotation);
            transform.localScale = scale;
        }
    }
}