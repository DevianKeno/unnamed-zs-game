using System.Threading.Tasks;

using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.Serialization;

using UZSG.Data;
using UZSG.Entities;
using UZSG.Players;

namespace UZSG.FPP
{
    public class FPPViewmodelController : MonoBehaviour
    {
        public Player Player;
        [Space]

        FPPViewmodelBobbing bobbing;
        FPPViewmodelSway sway;
        [SerializeField, FormerlySerializedAs("modelHolder")] Transform viewmodelsHolder;

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
                viewmodelsHolder.SetLocalPositionAndRotation(settings.PositionOffset, Quaternion.Euler(settings.RotationOffset));
            }
        }

        public async Task<ViewmodelComponent> LoadViewmodelAssetAsync(IViewmodelItem viewmodelItem)
        {
            if (false == viewmodelItem.HasViewmodel)
            {
                Game.Console.LogWarn($"Item '{(viewmodelItem as ItemData).Id}' has no viewmodel set.", true);
                return null;
            }
            
            var asyncOp = Addressables.LoadAssetAsync<GameObject>(viewmodelItem.ViewmodelAsset);
            await asyncOp.Task;

            if (asyncOp.Status == AsyncOperationStatus.Succeeded)
            {
                var model = Instantiate(asyncOp.Result, viewmodelsHolder);
                if (false == model.TryGetComponent<ViewmodelComponent>(out var viewmodelComponent))
                {
                    Game.Console.LogWarn($"Item '{(viewmodelItem as ItemData).Id}' is a viewmodel but has no Viewmodel Component.", true);
                    Destroy(model);
                    return null;
                }

                viewmodelComponent.ItemData = (ItemData) viewmodelItem;
                return viewmodelComponent;
            }

            return null;
        }
    }
}
