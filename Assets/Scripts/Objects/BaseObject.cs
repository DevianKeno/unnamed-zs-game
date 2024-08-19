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
    public abstract class BaseObject : MonoBehaviour, IAttributable, IPlaceable, IPickupable, ICollisionTarget, ISaveDataReadWrite<UserObjectSaveData>
    {
        [SerializeField] protected ObjectData objectData;
        public ObjectData ObjectData => objectData;

        [SerializeField] AttributeCollection attributes;
        public AttributeCollection Attributes => attributes;

        [SerializeField] protected AudioSourceController audioController;
        public AudioSourceController Audio => audioController;
        
        [SerializeField] protected Animator animator;
        public Animator Animator => animator;
        
        public event EventHandler<HitboxCollisionInfo> OnHit;


        #region Initializing methods

        protected virtual void Start()
        {
            InitializeAttributes();
            if (objectData.HasAudio) InitializeAudioController();
        }

        protected virtual void InitializeAttributes()
        {
            attributes = new();
            // attributes.InitializeFromData();
        }
        
        protected virtual void InitializeAudioController()
        {
            audioController.LoadAudioAssetsData(objectData.AudioAssetsData);
        }

        protected virtual void LoadGUIAsset(AssetReference asset, Action<ObjectGUI> onLoadCompleted = null)
        {
            if (!asset.IsSet())
            {
                Game.Console.LogAndUnityLog($"There's no GUI set for Workstation '{objectData.Id}'. This won't be usable unless otherwise you set its GUI.");
                return;
            }

            Addressables.LoadAssetAsync<GameObject>(asset).Completed += (a) =>
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

        public void ReadSaveJson(UserObjectSaveData saveData)
        {
            transform.SetPositionAndRotation(saveData.Transform.Position, saveData.Transform.Rotation);
            transform.localScale = saveData.Transform.LocalScale;
        }

        public UserObjectSaveData WriteSaveJson()
        {
            var saveData = new UserObjectSaveData()
            {
                Transform = new()
                {
                    Position = transform.position,
                    Rotation = transform.rotation,
                    LocalScale = transform.localScale,
                }
            };

            return saveData;
        }

        #endregion

        protected virtual void Destroy()
        {
            throw new NotImplementedException();
        }

        public void HitBy(HitboxCollisionInfo info)
        {
            throw new NotImplementedException();
        }
    }
}