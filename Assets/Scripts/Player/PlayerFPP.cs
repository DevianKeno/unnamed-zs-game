using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Animations;
using UZSG.FPP;
using UZSG.Systems;
using UZSG.Items;
using Cinemachine;

namespace UZSG.Player
{
    /// <summary>
    /// Handles the functionalities of the Player's first-person view.
    /// </summary>
    public class PlayerFPP : MonoBehaviour
    {
        GameObject _equipped;
        FPPAnimatable _animator;
        WeaponData _equippedData;
        float transitionDuration = 0.1f;
        Dictionary<int, GameObject> _cachedModels = new();
        Dictionary<int, IFPPVisible> _anims = new();
        
        PlayerEntity player;
        [SerializeField] FPPCamera _camera;
        public FPPCamera Camera => _camera;

        internal void Initialize()
        {
            
        }

        void Awake()
        {
            player = GetComponent<PlayerEntity>();
        }

        void Start()
        {
            player.sm.OnStateChanged += PlayerStateChangedCallback;
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
        public void Load(IFPPVisible obj, int index)
        {
            if (obj == null) return;

            GameObject go = Instantiate(obj.FPPModel, Camera.transform);
            _cachedModels.Add(index, go);
            _anims.Add(index, go.GetComponent<IFPPVisible>());
        }

        public void Equip(int index)
        {
            _equipped?.SetActive(false);
            _equipped = _cachedModels[index] ?? _equipped;
            

            _equipped.SetActive(true);
            _animator.Load(_anims[index]);
            _animator.Play("Equip");
        }
    }
}
