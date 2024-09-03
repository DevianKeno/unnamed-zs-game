using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

using UZSG.Systems;
using UZSG.Data;
using UZSG.Entities;
using UZSG.Players;

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

        FPPViewmodelBobbing bobbing;
        FPPViewmodelSway sway;
        [SerializeField] Transform modelHolder;

        void Awake()
        {
            bobbing = GetComponent<FPPViewmodelBobbing>();
            sway = GetComponent<FPPViewmodelSway>();
        }

        void Start()
        {
            Player.MoveStateMachine.OnTransition += OnPlayerMoveStateTransition;
        }

        void OnPlayerMoveStateTransition(StateMachine<MoveStates>.TransitionContext transition)
        {
            if (transition.To == MoveStates.Run)
            {
                // bobbing.Enabled = true;
                #region TODO: sway should still be enabled but rn the rotation from the Run Viewmodel Rotation collides with the sway animation
                sway.Enabled = false;
                #endregion
            }
            else
            {
                // bobbing.Enabled = true;
                sway.Enabled = true;
            }
        }

        /// <summary>
        /// Realigns the viewmodel model given its offset values.
        /// </summary>
        public void SetViewmodelSettings(ViewmodelSettings settings)
        {
            bobbing.SetViewmodelSettings(settings);
            if (settings.UseOffsets)
            {
                modelHolder.SetLocalPositionAndRotation(settings.PositionOffset, Quaternion.Euler(settings.RotationOffset));
            }
        }

        public struct LoadAssetReferenceInfo
        {
            public GameObject GameObject { get; set; }
            public AsyncOperationStatus Status { get; set; }
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
                        ItemData = (ItemData) viewmodel,
                        Model = model,
                        Settings = viewmodel.Settings,
                        ArmsAnimations = viewmodel.ArmsAnimations,

                        ModelAnimator = component.ModelAnimator,
                        CameraAnimator = component.CameraAnimator,
                        CameraAnimationSource = component.CameraAnimationSource,
                    };
                }
                
                Destroy(model);
                var msg = $"Item '{(viewmodel as ItemData).Id}' is a viewmodel but has no Viewmodel Component.";
                Game.Console.LogAndUnityLog(msg);
            }

            return null;
        }
    }
}
