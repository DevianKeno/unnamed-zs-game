using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

using UZSG.Systems;
using UZSG.Data;
using UZSG.Entities;

namespace UZSG.FPP
{
    public interface IViewmodelModifier
    {
        public Vector3 GetPositionOffset();
        public Quaternion GetRotationOffset();
    }

    public class FPPViewmodelController : MonoBehaviour
    {
        public Player Player;
        [Space]

        [SerializeField] Transform modelHolder;

        public struct LoadAssetReferenceInfo
        {
            public GameObject GameObject { get; set; }
            public AsyncOperationStatus Status { get; set; }
        }

        public async Task<Viewmodel> LoadViewmodelAssetAsync(IViewmodel viewmodel)
        {
            if (!viewmodel.HasViewmodel)
            {
                var msg = $"Item '{(viewmodel as ItemData).Id}' has no viewmodel set.";
                Game.Console.LogAndUnityLog(msg);
                return null;
            }

            LoadAssetReferenceInfo result = await LoadAssetReferenceAsync(viewmodel.Viewmodel);
            
            if (result.Status == AsyncOperationStatus.Succeeded)
            {
                var model = Instantiate(result.GameObject, modelHolder);
                if (model.TryGetComponent<ViewmodelComponent>(out var component))
                {
                    return new Viewmodel
                    {
                        ArmsAnimations = viewmodel.ArmsAnimations,
                        Model = model,
                        ItemData = viewmodel as ItemData,
                        ModelAnimator = component.ModelAnimator,
                        CameraAnimator = component.CameraAnimator,
                        CameraAnimationSource = component.CameraAnimationSource,
                    };
                }
                
                // Destroy(model);
                var msg = $"Item '{(viewmodel as ItemData).Id}' is a viewmodel but has no Viewmodel Component.";
                Game.Console.LogAndUnityLog(msg);
            }

            return null;
        }

        public async Task<LoadAssetReferenceInfo> LoadAssetReferenceAsync(AssetReference asset)
        {
            var op = Addressables.LoadAssetAsync<GameObject>(asset);
            await op.Task;
            return new LoadAssetReferenceInfo
            {
                GameObject = op.Result,
                Status = op.Status
            };
        }
    }
}
