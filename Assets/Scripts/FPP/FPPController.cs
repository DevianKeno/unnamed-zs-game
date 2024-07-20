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
        WeaponEquipped _currentlyEquippedWeapon;
        WeaponCategory _equippedWeaponCategory;
        bool _isPlayingAnimation;

        GameObject currentArms;
        public GameObject CurrentArms => currentArms;
        GameObject currentWeapon;
        public GameObject CurrentModel => currentWeapon;
        WeaponData currentWeaponData;
        public WeaponData CurrentWeaponData => _currentlyEquippedWeapon.WeaponData;

        MeleeWeaponStateMachine meleeWeaponStateMachine;
        RangedWeaponStateMachine rangedWeaponStateMachine;
        
        [Header("Controllers")]
        [SerializeField] FPPCameraController camController;
        public FPPCameraController CameraController => camController;
        [SerializeField] ViewmodelController viewmodelController;
        public ViewmodelController ViewmodelController => viewmodelController;

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
            camController.Initialize();
            
            var handsWeaponData = Resources.Load<WeaponData>("Data/weapons/hands");
            LoadAndEquip(handsWeaponData, HotbarIndex.Hands);
            Game.UI.ToggleCursor(false);
        }

        void InitializeEvents()
        {
            Player.smMove.OnStateChanged += OnPlayerMoveStateChanged;
            Player.smAction.OnStateChanged += OnPlayerActionStateChanged;
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
            if (_currentlyEquippedWeapon is RangedWeapon weapon)
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
                Game.Console.Log($"Arms viewmodel is missing an Animator component. No animations would be shown.");
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

            if (viewmodel.Weapon != null && viewmodel.Weapon.TryGetComponent(out WeaponEquipped weapon))
            {
                _currentlyEquippedWeapon = weapon;
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

            /// Idk if I really need to unsubscribe so it "resets"
            if (_currentlyEquippedWeapon is MeleeWeapon meleeWeapon)
            {
                meleeWeapon.StateMachine.OnStateChanged -= OnMeleeWeaponStateChanged;

                meleeWeapon.StateMachine.OnStateChanged += OnMeleeWeaponStateChanged;
            }
            else if (_currentlyEquippedWeapon is RangedWeapon rangedWeapon)
            {
                rangedWeapon.StateMachine.OnStateChanged -= OnRangedWeaponStateChanged;
                rangedWeapon.OnFire -= OnWeaponFired;

                rangedWeapon.StateMachine.OnStateChanged += OnRangedWeaponStateChanged;
                rangedWeapon.OnFire += OnWeaponFired;
            }
        }

        void OnPlayerMoveStateChanged(object sender, StateMachine<MoveStates>.StateChangedContext e)
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
        
        void OnPlayerActionStateChanged(object sender, StateMachine<ActionStates>.StateChangedContext e)
        {
            if (_currentlyEquippedWeapon == null) return;
            if (_isPlayingAnimation) return;

            if (_currentlyEquippedWeapon is MeleeWeapon meleeWeapon)
            {
                meleeWeapon.SetWeaponStateFromPlayerAction(e.To);
            }
            else if (_currentlyEquippedWeapon is RangedWeapon rangedWeapon)
            {
                rangedWeapon.SetWeaponStateFromPlayerAction(e.To);
            }
        }

        void OnMeleeWeaponStateChanged(object sender, StateMachine<MeleeWeaponStates>.StateChangedContext e)
        {
            throw new NotImplementedException();
        }

        void OnRangedWeaponStateChanged(object sender, StateMachine<RangedWeaponStates>.StateChangedContext e)
        {
            if (_currentlyEquippedWeapon == null) return;

            var animId = GetAnimIdFromState(e.To);
            if (!string.IsNullOrEmpty(animId))
            {
                _isPlayingAnimation = true;
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

        void HandleWeaponRecoil()
        {
            if (_currentlyEquippedWeapon is RangedWeapon weapon)
            {
                /// TECHNICAL DEBT
                // var attrs = currentWeaponData.RangedAttributes;
                // camController.RecoilRecoveryFactor = attrs.RecoilRecoveryFactor;
                // camController.AddVerticalRecoilMotion(attrs.VerticalRecoilFactor);
                // camController.AddHorizontaRecoilMotion(attrs.HorizontalRecoilFactor);
                /// camController should automatically reset the added recoil motion
            }
        }

        void UpdateHUD()
        {
            if (_currentlyEquippedWeapon is RangedWeapon weapon)
            {
                int ammoCount = weapon.CurrentRounds;
            }
        }

        IEnumerator FinishAnimation(float durationSeconds)
        {
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
