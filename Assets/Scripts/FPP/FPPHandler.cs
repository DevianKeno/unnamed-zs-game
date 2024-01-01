using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Animations;
using UZSG.Player;
using UZSG.Systems;
using UZSG.Items;

namespace UZSG.FPP
{
    /// <summary>
    /// Represents objects that are visible in first-person perspective.
    /// </summary>
    public interface IFPPVisible
    {
        public GameObject FPPModel { get; }
        public AnimatorController Controller { get; }
        public FPPAnimations Anims { get; }
    }

    /// <summary>
    /// Handles the functionalities of the first-person view.
    /// </summary>
    public class FPPHandler : MonoBehaviour
    {
        /// <summary>
        /// First-person camera.
        /// </summary>
        [SerializeField] Camera _camera;
        [SerializeField] PlayerEntity _player;
        GameObject _equipped;
        FPPAnimatable _animator;
        WeaponData _equippedData;
        float transitionDuration = 0.1f;
        Dictionary<int, GameObject> _cachedModels = new();
        Dictionary<int, IFPPVisible> _anims = new();

        void Awake()
        {
            _camera = GetComponent<Camera>();
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
        public void Load(IFPPVisible obj, int index)
        {
            if (obj == null) return;

            GameObject go = Instantiate(obj.FPPModel, _camera.transform);
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
