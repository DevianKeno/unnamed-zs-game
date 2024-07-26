using System.Threading.Tasks;

using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

using UZSG.Systems;
using UZSG.Items;
using UZSG.Entities;
using UZSG.Players;
using System;

namespace UZSG.FPP
{
    public class FPPViewmodelController : MonoBehaviour
    {
        public Player Player;

        [SerializeField] FPPCameraBobbing viewmodelBobbing;
        [SerializeField] FPPViewmodelBreathe viewmodelBreathe;
        [SerializeField] FPPViewmodelSway viewmodelSway;

        [SerializeField] Transform viewmodelHolder;

        internal void Initialize()
        {
            Player.MoveStateMachine.OnStateChanged += OnPlayerMoveStateChanged;
        }

        void OnPlayerMoveStateChanged(object sender, StateMachine<MoveStates>.StateChangedContext e)
        {
            if (e.To == MoveStates.Idle)
            {
                viewmodelBobbing.Enabled = false;
                viewmodelBreathe.Enabled = true;
            }
            else if (e.To == MoveStates.Walk)
            {
                viewmodelBreathe.Enabled = false;
                viewmodelBobbing.Enabled = true;
            }
        }

        public struct LoadAssetReferenceInfo
        {
            public GameObject GameObject { get; set; }
            public AsyncOperationStatus Status { get; set; }
        }

        public async Task<Viewmodel> LoadViewmodelAssetAsync(IFPPVisible fPPVisible)
        {
            GameObject model = null;

            if (fPPVisible.HasViewmodel)
            {
                LoadAssetReferenceInfo result = await LoadAssetReferenceAsync(fPPVisible.Viewmodel);
                
                if (result.Status == AsyncOperationStatus.Succeeded)
                {
                    model = Instantiate(result.GameObject, viewmodelHolder.transform);
                }
            }
            else
            {
                var msg = $"Item {(fPPVisible as ItemData).Id} has no viewmodel set.";
                Game.Console.Log(msg);
                Debug.LogWarning(msg);
            }

            return new Viewmodel
            {
                ArmsAnimations = fPPVisible.ArmsAnimations,
                Model = model,
                ItemData = fPPVisible as ItemData,
            };
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
