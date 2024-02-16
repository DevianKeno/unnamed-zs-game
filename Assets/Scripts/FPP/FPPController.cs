using System.Collections.Generic;
using UnityEngine;
using UZSG.Systems;
using UZSG.Entities;
using UZSG.PlayerCore;

namespace UZSG.FPP
{
    /// <summary>
    /// Handles the functionalities of the Player's first-person perspective.
    /// </summary>
    public class PlayerFPP : MonoBehaviour
    {
        struct CachedModel
        {
            public GameObject ArmsModel;
            public GameObject Model;
        }
        public bool FollowCamera = true;
        GameObject _equipped;
        FPPModel _animator;
        Dictionary<int, CachedModel> _cachedModels = new();
        Dictionary<int, IFPPVisible> _anims = new();
        FPPModel currentModel;
        Player _player;
        public Player Player => _player;
        ArmsController _arms;
        public ArmsController Arms => _arms;
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
            if (FollowCamera)
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
            _player.sm.OnStateChanged += PlayerStateChangedCallback;
            Game.UI.ToggleCursor(false);
        }

        void PlayerStateChangedCallback(object sender, StateMachine<PlayerStates>.StateChangedArgs e)
        {
            if (_equipped == null) return;
            // _animator.Play(_anims[1].Anims.Idle);
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
                ArmsModel = armsModel,
                Model = model

            });

            _anims.Add(slot, armsModel.GetComponent<IFPPVisible>());
        }

        public void Equip(int slot)
        {
            _equipped?.SetActive(false);
            // _equipped = _cachedModels[slot] ?? _equipped;
            
            _equipped.SetActive(true);
            _animator.Load(_anims[slot]);
            _animator.Play("Equip");
        }
    }
}
