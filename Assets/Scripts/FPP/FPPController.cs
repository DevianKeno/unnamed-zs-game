using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using UZSG.Systems;
using UZSG.Entities;
using UZSG.Players;
using UZSG.Inventory;
using UZSG.Items.Weapons;

namespace UZSG.FPP
{
    /// <summary>
    /// Handles the functionalities of the Player's first-person perspective.
    /// </summary>
    public class FPPController : MonoBehaviour
    {
        public Player Player;
        
        Dictionary<HotbarIndex, Viewmodel> _cachedViewmodels = new();
        HotbarIndex currentlyEquippedIndex;
        public HotbarIndex CurrentlyEquippedIndex => currentlyEquippedIndex;
        HotbarIndex lastEquippedIndex;
        EquippedWeapon _currentlyEquippedWeapon;
        WeaponCategory _equippedWeaponCategory;
        bool _isPlayingAnimation;

        GameObject currentArms;
        public GameObject CurrentArms => currentArms;
        GameObject currentWeapon;
        public GameObject CurrentModel => currentWeapon;
        WeaponData currentWeaponData;
        public WeaponData CurrentWeaponData => _currentlyEquippedWeapon.WeaponData;
        
        [Header("Controllers")]
        [SerializeField] FPPCameraController cameraController;
        public FPPCameraController CameraController => cameraController;
        [SerializeField] FPPViewmodelController viewmodelController;

        Animator armsAnimator;
        Animator weaponAnimator;
        Animator cameraAnimator;
        
        void Awake()
        {
            Player = GetComponent<Player>();
        }

        internal void Initialize()
        {
            InitializeEvents();
            cameraController.Initialize();
            
            var handsWeaponData = Resources.Load<WeaponData>("Data/weapons/hands");
            LoadAndEquip(handsWeaponData, HotbarIndex.Hands);
            Game.UI.ToggleCursor(false);
        }

        void InitializeEvents()
        {
            Player.MoveStateMachine.OnStateChanged += OnPlayerMoveStateChanged;
            Player.ActionStateMachine.OnStateChanged += OnPlayerActionStateChanged;
        }

        public delegate void OnLoadViewModelCompleted(Viewmodel viewmodel);
        
        /// <summary>
        /// Cache FPP model and data.
        /// </summary>
        public void LoadAndEquip(IFPPVisible obj, HotbarIndex index, OnLoadViewModelCompleted callback = null)
        {
            viewmodelController.LoadViewmodelAsync(obj, (viewmodel) =>
            {
                if (!_cachedViewmodels.ContainsKey(index))
                {
                    _cachedViewmodels[index] = viewmodel;
                }

                EquipIndex(index);
                callback?.Invoke(viewmodel);
            });
        }      

        public void EquipIndex(HotbarIndex index)
        {
            if (_cachedViewmodels.ContainsKey(index))
            {
                currentlyEquippedIndex = index;
                EquipViewmodel(_cachedViewmodels[index]);
            }
        }

        public void PerformReload()
        {
            if (_isPlayingAnimation) return;
            if (_currentlyEquippedWeapon is GunWeapon weapon)
            {
                var animLengthSeconds = GetAnimationClipLength(weaponAnimator, "reload");
                weapon.TryReload(animLengthSeconds);
            }
        }

        public void Unholster()
        {
            if (_isPlayingAnimation) return;

            /// Swaps
            if (currentlyEquippedIndex == HotbarIndex.Hands)
            {
                EquipIndex(lastEquippedIndex);
            }
            else
            {
                lastEquippedIndex = currentlyEquippedIndex;
                EquipIndex(HotbarIndex.Hands);
            }
        }

        void ReplaceViewmodelArms(Viewmodel viewmodel)
        {
            if (currentArms != null)
            {
                currentArms.gameObject.SetActive(false);
            }
            
            currentArms = viewmodel.Arms;
            if (viewmodel.Arms != null && viewmodel.Arms.TryGetComponent(out Animator component))
            {
                armsAnimator = component;
            }
            else
            {
                // Game.Console.Log($"Arms viewmodel is missing an Animator component. No animations would be shown.");
                Debug.LogWarning($"Arms viewmodel is missing an Animator component. No animations would be shown.");
            }
        }

        void ReplaceViewmodelWeapon(Viewmodel viewmodel)
        {
            if (currentWeapon != null)
            {
                currentWeapon.gameObject.SetActive(false);
            }

            currentWeapon = viewmodel.Weapon;
            if (viewmodel.Weapon != null && viewmodel.Weapon.TryGetComponent(out Animator component))
            {
                weaponAnimator = component;
            }
            else
            {
                // Game.Console.Log($"Weapon viewmodel is missing an Animator component. No animations would be shown.");
                Debug.LogWarning($"Weapon viewmodel is missing an Animator component. No animations would be shown.");
            }

            if (viewmodel.Weapon != null && viewmodel.Weapon.TryGetComponent(out EquippedWeapon weapon))
            {
                _currentlyEquippedWeapon = weapon;
                _currentlyEquippedWeapon.Owner = Player;
                _currentlyEquippedWeapon.Initialize();
            }
            else
            {
                // Game.Console.Log($"Weapon viewmodel is missing an Animator component. No animations would be shown.");
                Debug.LogWarning($"Weapon viewmodel is missing an Animator component. No animations would be shown.");
            }
        }

        void EquipViewmodel(Viewmodel viewmodel)
        {
            ReplaceViewmodelArms(viewmodel);
            ReplaceViewmodelWeapon(viewmodel);

            currentWeaponData = viewmodel.WeaponData;

            viewmodelController.ReplaceViewmodel(viewmodel);
            if (currentWeaponData != null)
            {
                _equippedWeaponCategory = currentWeaponData.Category;
            }

            armsAnimator?.CrossFade("equip", 0.1f, 0, 0f);
            weaponAnimator?.CrossFade("equip", 0.1f, 0, 0f);

            HandleEquippedWeaponEvents();
        }

        void HandleEquippedWeaponEvents()
        {
            /// Idk if I really need to unsubscribe so it "resets"
            if (_currentlyEquippedWeapon is MeleeWeapon meleeWeapon)
            {
                meleeWeapon.StateMachine.OnStateChanged -= OnMeleeWeaponStateChanged;

                meleeWeapon.StateMachine.OnStateChanged += OnMeleeWeaponStateChanged;
            }
            else if (_currentlyEquippedWeapon is GunWeapon rangedWeapon)
            {
                rangedWeapon.StateMachine.OnStateChanged -= OnRangedWeaponStateChanged;
                rangedWeapon.OnFire -= OnWeaponFired;

                rangedWeapon.StateMachine.OnStateChanged += OnRangedWeaponStateChanged;
                rangedWeapon.OnFire += OnWeaponFired;
            }
        }

        
        #region Event callbacks

        void OnPlayerMoveStateChanged(object sender, StateMachine<MoveStates>.StateChangedContext e)
        {
            switch (e.To)
            {                
                case MoveStates.Idle:
                    cameraController.Animator.CrossFade("idle", 0.1f);
                    break;

                case MoveStates.Walk:
                    cameraController.Animator.speed = 1f;
                    cameraController.Animator.CrossFade("forward_bob", 0.1f);
                    break;

                case MoveStates.Run:
                    cameraController.Animator.speed = 1.6f;
                    cameraController.Animator.CrossFade("forward_bob", 0.1f);
                    break;
            }
        }
        
        void OnPlayerActionStateChanged(object sender, StateMachine<ActionStates>.StateChangedContext e)
        {
            if (_currentlyEquippedWeapon == null) return;
            if (_isPlayingAnimation) return;

            _currentlyEquippedWeapon.SetWeaponStateFromPlayerAction(e.To);
        }

        void OnMeleeWeaponStateChanged(object sender, StateMachine<MeleeWeaponStates>.StateChangedContext e)
        {
            throw new NotImplementedException();
        }

        void OnRangedWeaponStateChanged(object sender, StateMachine<GunWeaponStates>.StateChangedContext e)
        {
            if (_currentlyEquippedWeapon == null) return;

            var animId = GetAnimIdFromState(e.To);
            if (!string.IsNullOrEmpty(animId))
            {
                armsAnimator.Play(animId, 0, 0f);
                weaponAnimator.Play(animId, 0, 0f);

                var animLengthSeconds = GetAnimationClipLength(weaponAnimator, animId);
                StartCoroutine(FinishAnimation(animLengthSeconds));
            }
        }
        
        void OnWeaponFired()
        {
            HandleWeaponRecoil();
            UpdateHUD();
        }

        #endregion


        void HandleWeaponRecoil()
        {
            if (_currentlyEquippedWeapon is GunWeapon weapon)
            {
                var recoilInfo = currentWeaponData.RangedAttributes.RecoilAttributes;
                cameraController.AddRecoilMotion(recoilInfo);
            }
        }

        void UpdateHUD()
        {
            if (_currentlyEquippedWeapon is GunWeapon weapon)
            {
                int ammoCount = weapon.CurrentRounds;
            }
        }

        IEnumerator FinishAnimation(float durationSeconds)
        {
            _isPlayingAnimation = true;
            yield return new WaitForSeconds(durationSeconds);
            _isPlayingAnimation = false;
            yield return null;
        }


        #region Helper functions

        float GetAnimationClipLength(Animator animator, string name)
        {
            foreach (var clip in animator.runtimeAnimatorController.animationClips)
            {
                if (clip.name == name) return clip.length;
            }
            return 0f;
        }

        string GetAnimIdFromState(Enum value)
        {
            return value.ToString().ToLower();
        }

        #endregion
    }
}
