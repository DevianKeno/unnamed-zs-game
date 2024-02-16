using System.Collections.Generic;
using UnityEngine;
using UZSG.Systems;
using UZSG.Entities;
using UZSG.PlayerCore;
using UnityEditor.Animations;
using System;

namespace UZSG.FPP
{
    /// <summary>
    /// Handles the functionalities of the Player's first-person perspective.
    /// </summary>
    public class FPPController : MonoBehaviour
    {
        public class CachedModel
        {
            public FPPModel ArmsModel;
            public FPPModel Model;
        }
        public bool ModelFollowsCamera = true;
        CachedModel _equipped;
        Animator _animator;
        Dictionary<int, CachedModel> _cachedModels = new();
        Dictionary<int, IFPPVisible> _anims = new();
        FPPModel currentModel;
        Player _player;
        public Player Player => _player;
        ArmsController _arms;
        public ArmsController Arms => _arms;
        Animator _armsAnimator;
        FPPCamera _camera;
        public FPPCamera Camera => _camera;
        [SerializeField] Transform CameraHolder;
        [SerializeField] Transform ArmsHolder;
        [SerializeField] Transform ModelHolder;

        internal void Init()
        {
        }

        void Awake()
        {
            _player = GetComponent<Player>();
            _arms = GetComponent<ArmsController>();
            _camera = GetComponent<FPPCamera>();
        }

        void Update()
        {
            if (ModelFollowsCamera)
            {
                /// For some unknown reason, this causes a smooth "follow" effect
                /// for the FPP model in relation to the camera's movement
                var follow = _player.MainCamera.transform.rotation;
                CameraHolder.transform.rotation = follow;
                ArmsHolder.transform.rotation = follow;
                ModelHolder.transform.rotation = follow;
            }
        }

        void Start()
        {
            _player.smAction.OnStateChanged += ActionStateChanged;
            Game.UI.ToggleCursor(false);
        }

        private void ActionStateChanged(object sender, StateMachine<ActionStates>.StateChanged e)
        {
            if (_equipped == null) return;

            if (e.To == ActionStates.Primary)
            {
                PlayAnimation(_equipped, "attack_left");

            } else if (e.To == ActionStates.SecondaryHold)
            {
                PlayAnimation(_equipped, "stance");
            }
        }

        /// <summary>
        /// Plays an animation for the currently equipped.
        /// </summary>
        public void PlayAnimation(CachedModel model, string anim)
        {
            _equipped.ArmsModel.Play(anim);
            _equipped.Model.Play(anim);
        }

        /// <summary>
        /// Cache model and data.
        /// </summary>
        public void LoadModel(IFPPVisible obj, int slot)
        {
            if (obj == null) return;
 
            var armsModel = Instantiate(obj.ArmsModel, ArmsHolder);
            var model = Instantiate(obj.Model, ModelHolder);
            
            _cachedModels.Add(slot, new CachedModel()
            {
                ArmsModel = armsModel.GetComponent<FPPModel>(),
                Model = model.GetComponent<FPPModel>()
            });

            // _animator = armsModel.GetComponent<Animator>();
            // _anims.Add(slot, armsModel.GetComponent<IFPPVisible>());
        }

        public void Equip(int slot)
        {
            if (_cachedModels.ContainsKey(slot))
            {
                var toEquip = _cachedModels[slot];

                if (_equipped == null)
                {
                    _equipped = toEquip;
                    PlayAnimation(_equipped, "equip");
                } else
                {
                    if (_equipped.Equals(toEquip))
                    {
                        Debug.Log("dequip");
                    } else
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
