using System.Collections.Generic;

using UnityEngine;

using UZSG.Systems;
using UZSG.Entities;
using UZSG.Players;
using UZSG.Inventory;

namespace UZSG.FPP
{
    /// <summary>
    /// Handles the functionalities of the Player's first-person perspective.
    /// </summary>
    public class FPPController : MonoBehaviour
    {
        public class CachedModel
        {
            public FPPModel Model;
        }
        
        CachedModel _equipped;
        Dictionary<int, CachedModel> _cachedModels = new();

        [Header("Components")]
        public Player Player;
        [SerializeField] FPPCameraController camController;
        public FPPCameraController CameraController => camController;
        [SerializeField] ArmsController armsController;
        public ArmsController ArmsController => armsController;
        [SerializeField] Transform modelHolder;

        internal void Initialize()
        {
        }

        void Awake()
        {
            Player = GetComponent<Player>();
        }

        void Start()
        {
            Player.smMove.OnStateChanged += MoveStateChanged;
            Player.smAction.OnStateChanged += ActionStateChanged;
            Game.Entity.OnEntitySpawn += PlayerSpawnedCallback;

            Game.UI.ToggleCursor(false);
        }

        void MoveStateChanged(object sender, StateMachine<MoveStates>.StateChanged e)
        {
            switch (e.To)
            {                
                case MoveStates.Idle:
                    camController.Animator.CrossFade("idle", 0.1f);
                    break;

                case MoveStates.Walk:
                    camController.Animator.speed = 1f;
                    camController.Animator.CrossFade("forward_bob", 0.1f);
                    break;

                case MoveStates.Run:
                    camController.Animator.speed = 1.6f;
                    camController.Animator.CrossFade("forward_bob", 0.1f);
                    break;
            }
        }

        void PlayerSpawnedCallback(object sender, EntityManager.EntitySpawnedContext e)
        {
            if (e.Entity is Player player)
            {
                BindPlayer(player);
            }
        }

        public void BindPlayer(Player player)
        {
            Player = player;
            Game.Console.LogDebug($"Bound player {player} to FPP controller");
        }

        private void ActionStateChanged(object sender, StateMachine<ActionStates>.StateChanged e)
        {
            if (_equipped == null) return;

            if (e.To == ActionStates.Primary)
            {
                PlayAnimation("attack_left");

            } else if (e.To == ActionStates.SecondaryHold)
            {
                PlayAnimation("stance");
            }
        }

        /// <summary>
        /// Plays an animation for the currently equipped.
        /// </summary>
        public void PlayAnimation(string anim)
        {
            armsController.PlayAnimation(anim);
            // _equipped.ArmsModel.Animator.Play(anim);
            // _equipped.Model.Animator.Play(anim);
            // camAnimator.Play(anim);
        }

        /// <summary>
        /// Cache model and data.
        /// </summary>
        public void LoadModel(IFPPVisible obj, HotbarIndex value)
        {
            if (obj == null) return;
            // if (obj.Model == null) return;
            
            var model = Instantiate(obj.Model);
            model.transform.SetParent(modelHolder.transform);
            
            FPPModel fppModel = model.GetComponent<FPPModel>();
            _cachedModels.Add((int) value, new CachedModel()
            {
                Model = fppModel
            });
            
            armsController.LoadAnimationController(obj.ArmsAnimController);
            // _animator = armsModel.GetComponent<Animator>();
            // _anims.Add(slot, armsModel.GetComponent<IFPPVisible>());
        }

        public void Equip(HotbarIndex value)
        {
            Equip((int) value);
        }

        public void Equip(int slotIndex)
        {
            if (_cachedModels.ContainsKey(slotIndex))
            {
                var toEquip = _cachedModels[slotIndex];

                // cameraOffset = Vector3.zero;
                // if (toEquip.ArmsModel.HasCameraAnims)
                // {
                //     cameraOffset = toEquip.ArmsModel.Camera.transform.rotation.eulerAngles;
                // }

                if (_equipped == null)
                {
                    _equipped = toEquip;
                    PlayAnimation("equip");
                }
                else
                {
                    if (_equipped.Equals(toEquip))
                    {
                        Debug.Log("dequip");
                    }
                    else
                    {
                        Debug.Log("switched");
                    }
                }
            }
            // _equipped?.SetActive(false);
            // _equipped = _cachedModels[slot] ?? _equipped;
            
            // _equipped.SetActive(true);
        }
    }
}
