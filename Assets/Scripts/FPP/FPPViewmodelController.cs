using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

using UZSG.Systems;
using UZSG.Items;
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

        FPPViewmodelBobbing viewmodelBobbing;
        FPPViewmodelBreathe viewmodelBreathe;
        FPPViewmodelSway viewmodelSway;

        void Awake()
        {
            viewmodelBobbing = GetComponent<FPPViewmodelBobbing>();
            viewmodelBreathe = GetComponent<FPPViewmodelBreathe>();
            viewmodelSway = GetComponent<FPPViewmodelSway>();
        }

        List<IViewmodelModifier> modifiers = new();
        Vector3 positionOffset;
        Quaternion rotationOffset;

        void Update()
        {
            // positionOffset = Vector3.zero;
            // rotationOffset = Quaternion.identity;

            // foreach (var modifier in modifiers)
            // {
            //     positionOffset += modifier.GetPositionOffset();
            //     rotationOffset *= modifier.GetRotationOffset();
            // }

            // transform.SetLocalPositionAndRotation(positionOffset, rotationOffset);
        }

        internal void Initialize()
        {
            Player.MoveStateMachine.OnStateChanged += OnPlayerMoveStateChanged;
        }

        void OnPlayerMoveStateChanged(object sender, StateMachine<MoveStates>.StateChangedContext e)
        {
            // if (e.To == MoveStates.Idle)
            // {
            //     viewmodelBobbing.Enabled = false;
            //     viewmodelBreathe.Enabled = true;
            // }
            // else if (e.To == MoveStates.Walk)
            // {
            //     viewmodelBreathe.Enabled = false;
            //     viewmodelBobbing.Enabled = true;
            // }
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
                    model = Instantiate(result.GameObject, transform);
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
