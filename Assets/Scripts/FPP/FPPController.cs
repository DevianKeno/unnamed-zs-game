using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using UZSG.Systems;
using UZSG.Entities;
using UZSG.Players;
using UZSG.Inventory;
using UZSG.Items.Weapons;
using UZSG.Items;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace UZSG.FPP
{
    /// <summary>
    /// Handles the functionalities of the Player's first-person perspective.
    /// </summary>
    public class FPPController : MonoBehaviour
    {
        public Player Player;
        
        [Space(10)]
        [SerializeField] HeldItemController heldItem;
        Dictionary<HotbarIndex, HeldItemController> _cachedHeldItems = new();
        Dictionary<string, Viewmodel> _cachedViewmodelsById = new();

        HotbarIndex currentlyEquippedIndex;
        public HotbarIndex CurrentlyEquippedIndex => currentlyEquippedIndex;
        HotbarIndex lastEquippedIndex;
        bool _isPlayingAnimation;

        GameObject currentArms;
        public GameObject CurrentArms => currentArms;
        GameObject currentWeapon;
        public GameObject CurrentModel => currentWeapon;
        
        [Header("Controllers")]
        [SerializeField] FPPCameraController cameraController;
        public FPPCameraController CameraController => cameraController;
        [SerializeField] FPPViewmodelController viewmodelController;
        [SerializeField] GunMuzzleController gunMuzzleController;

        [SerializeField] GameObject heldItems;
        Animator armsAnimator;
        Animator weaponAnimator;
        Animator cameraAnimator;
        
        [Space(10)]
        [SerializeField] AssetReference gunWeaponControllerPrefab;
        
        void Awake()
        {
            Player = GetComponent<Player>();
        }

        internal void Initialize()
        {
            InitializeEvents();
            cameraController.Initialize();
            LoadAndEquipHands();
            Game.UI.ToggleCursor(false);
        }

        void InitializeEvents()
        {
            Player.MoveStateMachine.OnStateChanged += OnPlayerMoveStateChanged;
            Player.ActionStateMachine.OnStateChanged += OnPlayerActionStateChanged;
        }

        void LoadAndEquipHands()
        {
            var handsWeaponData = Game.Items.GetItemData("hands");
            LoadViewmodel(handsWeaponData, HotbarIndex.Hands, equip: true);
        }

        public delegate void OnLoadViewModelCompleted(Viewmodel viewmodel);

        /// <summary>
        /// Cache FPP model and data.
        /// </summary>
        public void LoadViewmodel(ItemData item, HotbarIndex index, bool equip = true, OnLoadViewModelCompleted callback = null)
        {
            if (item is not IFPPVisible FPPObject) return;
            if (FPPObject.HasViewmodel)
            {
                viewmodelController.LoadViewmodelAsync(FPPObject, (viewmodel) =>
                {
                    /// Store loaded viewmodel
                    if (!_cachedViewmodelsById.ContainsKey(item.Id))
                    {
                        _cachedViewmodelsById[item.Id] = viewmodel;
                    }
                    if (equip)
                    {
                        EquipViewmodel(viewmodel); /// Equip viewmodel on finish load :)
                    }
                    callback?.Invoke(viewmodel);
                });
            }
        }

        public void InitializeHeldItem(Item item, HotbarIndex index, Action onDoneInitialize = null)
        {
            if (item.Data is WeaponData weaponData)
            {
                if (weaponData.Category == WeaponCategory.Melee)
                {
                    LoadHeldItemController<MeleeWeaponController>();
                }
                else if (weaponData.Category == WeaponCategory.Ranged)
                {
                    LoadHeldItemController<GunWeaponController>((gunWeapon) =>
                    {
                        gunWeapon.name = $"{item.Name}";
                        gunWeapon.transform.parent = heldItems.transform;
                        gunWeapon.ItemData = item.Data;
                        gunWeapon.Owner = Player;
                        gunWeapon.Initialize();
                        _cachedHeldItems[index] = gunWeapon;

                        HoldItem(index);
                        onDoneInitialize?.Invoke();
                    });
                }
            }
            else if (item.Data.Subtype == ItemSubtype.Consumable)
            {
                throw new NotImplementedException();
            }
        }

        public void HoldItem(HotbarIndex index)
        {
            if (_cachedHeldItems.ContainsKey(index))
            {
                heldItem = _cachedHeldItems[index];
            }
            else
            {
                heldItem = null;
            }
            ReinitializeHeldItemEvents();
        }
        
        T LoadHeldItemController<T>(Action<T> callback = null) where T : HeldItemController
        {
            Addressables.LoadAssetAsync<GameObject>(gunWeaponControllerPrefab).Completed += (a) =>
            {
                if (a.Status == AsyncOperationStatus.Succeeded)
                {
                    var controller = Instantiate(a.Result).GetComponent<T>();
                    callback.Invoke(controller);
                }
            };
            return default;
        }

        public void EquipHotbarIndex(HotbarIndex index)
        {
            if (_isPlayingAnimation) return;
            if (currentlyEquippedIndex == index) return;
            if (!Player.Inventory.Hotbar.TryGetSlot((int) index, out var slot)) return;
            if (slot.IsEmpty) return;

            currentlyEquippedIndex = index;
            UnloadCurrentViewmodel();
            if (_cachedViewmodelsById.ContainsKey(slot.Item.Id))
            {
                EquipViewmodel(_cachedViewmodelsById[slot.Item.Id]);
            }
            HoldItem(index);
        }

        public void Unholster()
        {
            if (_isPlayingAnimation) return;

            /// Swaps
            if (currentlyEquippedIndex == HotbarIndex.Hands)
            {
                EquipHotbarIndex(lastEquippedIndex);
            }
            else
            {
                lastEquippedIndex = currentlyEquippedIndex;
                EquipHotbarIndex(HotbarIndex.Hands);
            }
        }

        void UnloadCurrentViewmodel()
        {
            if (currentArms != null)
            {
                currentArms.gameObject.SetActive(false);
            }
            currentArms = null;
            if (currentWeapon != null)
            {
                currentWeapon.gameObject.SetActive(false);
            }
            currentWeapon = null;
        }

        void EquipViewmodel(Viewmodel viewmodel)
        {
            UnloadCurrentViewmodel();
            /// These two functions loads the animator, SALT
            LoadViewmodelArms(viewmodel);
            LoadViewmodelWeapon(viewmodel);

            viewmodelController.ReplaceViewmodel(viewmodel);

            armsAnimator?.CrossFade("equip", 0.1f, 0, 0f);
            weaponAnimator?.CrossFade("equip", 0.1f, 0, 0f);
        }

        void LoadViewmodelArms(Viewmodel viewmodel)
        {
            if (viewmodel.Arms != null && viewmodel.Arms.TryGetComponent(out Animator component))
            {
                currentArms = viewmodel.Arms;
                armsAnimator = component;
            }
            else
            {
                // Game.Console.Log($"Arms viewmodel is missing an Animator component. No animations would be shown.");
                Debug.LogWarning($"Arms viewmodel is missing an Animator component. No animations would be shown.");
            }
        }

        void LoadViewmodelWeapon(Viewmodel viewmodel)
        {
            if (viewmodel.Weapon == null)
            {
                gunMuzzleController = null;
                return;
            }
            
            if (viewmodel.Weapon.TryGetComponent(out Animator component))
            {
                currentWeapon = viewmodel.Weapon;
                weaponAnimator = component;
            }
            else
            {
                // Game.Console.Log($"Weapon viewmodel is missing an Animator component. No animations would be shown.");
                Debug.LogWarning($"Weapon viewmodel is missing an Animator component. No animations would be shown.");
            }

            if (viewmodel.Weapon.TryGetComponent(out FPPGunModel gunModel))
            {
                gunMuzzleController = gunModel.MuzzleController;
            }
        }

        void ReinitializeHeldItemEvents()
        {
            /// Idk if I really need to unsubscribe so it "resets"
            if (heldItem is MeleeWeaponController meleeWeapon)
            {
                meleeWeapon.StateMachine.OnStateChanged -= OnMeleeWeaponStateChanged;

                meleeWeapon.StateMachine.OnStateChanged += OnMeleeWeaponStateChanged;
            }
            else if (heldItem is GunWeaponController rangedWeapon)
            {
                rangedWeapon.StateMachine.OnStateChanged -= OnRangedWeaponStateChanged;
                rangedWeapon.OnFire -= OnWeaponFired;

                rangedWeapon.StateMachine.OnStateChanged += OnRangedWeaponStateChanged;
                rangedWeapon.OnFire += OnWeaponFired;
            }
        }

        public void PerformReload()
        {
            if (_isPlayingAnimation) return;

            if (heldItem is IReloadable reloadableWeapon)
            {
                var reloadDuration = GetAnimationClipLength(weaponAnimator, "reload");
                reloadableWeapon.TryReload(reloadDuration);
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
            if (heldItem == null) return;
            if (_isPlayingAnimation) return;

            if (heldItem != null)
            {
                heldItem.SetStateFromAction(e.To);
            }
        }

        void OnMeleeWeaponStateChanged(object sender, StateMachine<MeleeWeaponStates>.StateChangedContext e)
        {
            throw new NotImplementedException();
        }

        void OnRangedWeaponStateChanged(object sender, StateMachine<GunWeaponStates>.StateChangedContext e)
        {
            if (heldItem == null) return;

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
            gunMuzzleController?.Fire();
            HandleWeaponRecoil();
            UpdateHUD();
        }

        #endregion


        void HandleWeaponRecoil()
        {
            if (heldItem is GunWeaponController weapon)
            {
                var weaponData = weapon.ItemData as WeaponData;
                var recoilInfo = weaponData.RangedAttributes.RecoilAttributes;
                cameraController.AddRecoilMotion(recoilInfo);
            }
        }

        void UpdateHUD()
        {
            if (heldItem is GunWeaponController weapon)
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
