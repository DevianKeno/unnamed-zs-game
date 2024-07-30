using System;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using UZSG.Systems;
using UZSG.Entities;
using UZSG.Players;
using UZSG.Inventory;
using UZSG.Items.Weapons;
using UZSG.Items;

namespace UZSG.FPP
{
    /// <summary>
    /// Handles the functionalities of the Player's first-person perspective.
    /// </summary>
    public class FPPController : MonoBehaviour
    {
        public Player Player;
        [Space]
        
        [SerializeField] HeldItemController heldItem;
        [SerializeField] GameObject heldItemsContainer;
        Dictionary<HotbarIndex, HeldItemController> _cachedHeldItems = new();
        Dictionary<string, Viewmodel> _cachedViewmodelsById = new();

        HotbarIndex currentlyEquippedIndex;
        public HotbarIndex CurrentlyEquippedIndex => currentlyEquippedIndex;
        HotbarIndex lastEquippedIndex;
        bool _isPlayingAnimation;

        Viewmodel currentViewmodel;
        public Viewmodel CurrentViewmodel => currentViewmodel;
        
        [Header("Controllers")]
        [SerializeField] FPPArmsController armsController;
        [SerializeField] FPPViewmodelController viewmodelController;
        [SerializeField] FPPCameraInput cameraController;
        public FPPCameraInput CameraController => cameraController;
        [SerializeField] GunMuzzleController gunMuzzleController;

        public event Action<HeldItemController> OnChangeHeldItem;

        Animator viewmodelAnimator;
        Animator cameraAnimator;
        
        [Header("Held Item Controller Prefabs")]
        [SerializeField] GameObject meleeWeaponControllerPrefab;
        [SerializeField] GameObject gunWeaponControllerPrefab;
        
        void Awake()
        {
            Player = GetComponent<Player>();
        }

        internal void Initialize()
        {
            InitializeEvents();
            cameraController.Initialize();
            viewmodelController.Initialize();
            LoadAndEquipHands();
            Game.UI.ToggleCursor(false);
        }

        void InitializeEvents()
        {
            Player.MoveStateMachine.OnStateChanged += OnPlayerMoveStateChanged;
            Player.ActionStateMachine.OnStateChanged += OnPlayerActionStateChanged;
        }

        public void HoldItem(Item item, HotbarIndex index)
        {
            LoadFPPItem(item.Data, index, equip: true);
            LoadHeldItem(item, index, () =>
            {
                Player.FPP.EquipHotbarIndex(index);
            });
        }

        void LoadAndEquipHands()
        {
            var armsItem = new Item(Game.Items.GetItemData("arms"));
            HoldItem(armsItem, HotbarIndex.Hands);
        }

        public delegate void OnLoadViewModelCompleted(Viewmodel viewmodel);

        /// <summary>
        /// Cache FPP model and data.
        /// </summary>
        async void LoadFPPItem(ItemData item, HotbarIndex index, bool equip = true)
        {
            if (item is not IFPPVisible fPPObject) return;
            
            await LoadViewmodelAssetAsync(fPPObject, equip);
        }

        async Task<Viewmodel> LoadViewmodelAssetAsync(IFPPVisible fPPObject, bool equip)
        {
            var item = fPPObject as ItemData;
            var viewmodel = await viewmodelController.LoadViewmodelAssetAsync(fPPObject);

            /// Cache loaded viewmodel
            if (!_cachedViewmodelsById.ContainsKey(item.Id))
            {
                _cachedViewmodelsById[item.Id] = viewmodel;
            }
            if (equip)
            {
                /// Equip viewmodel on finish load :)
                EquipViewmodel(viewmodel);
            }
            return viewmodel;
        }

        void LoadHeldItem(Item item, HotbarIndex index, Action onDoneInitialize = null)
        {
            if (item.Data is WeaponData weaponData)
            {
                if (weaponData.Category == WeaponCategory.Melee)
                {
                    LoadHeldItemControllerAsync<MeleeWeaponController>(meleeWeaponControllerPrefab, (meleeWeapon) =>
                    {
                        InitializeHeldItemController(item, meleeWeapon, index);
                        onDoneInitialize?.Invoke();
                    });
                }
                else if (weaponData.Category == WeaponCategory.Ranged)
                {
                    LoadHeldItemControllerAsync<GunWeaponController>(gunWeaponControllerPrefab, (gunWeapon) =>
                    {
                        InitializeHeldItemController(item, gunWeapon, index);
                        onDoneInitialize?.Invoke();
                    });
                }
                return;
            }

            if (item.Data.Subtype == ItemSubtype.Consumable)
            {
                throw new NotImplementedException();
            }
        }

        void InitializeHeldItemController(Item item ,HeldItemController controller, HotbarIndex index)
        {
            controller.transform.parent = heldItemsContainer.transform;
            controller.ItemData = item.Data;
            controller.Owner = Player;
            controller.Initialize();
            _cachedHeldItems[index] = controller;
            controller.name = $"{item.Name} (Held Item)";

            HoldItemByIndex(index);
            InitializeHeldItem();
        }

        void HoldItemByIndex(HotbarIndex index)
        {
            if (heldItem != null)
            {
                heldItem.gameObject.SetActive(false);
            }

            if (_cachedHeldItems.ContainsKey(index))
            {
                heldItem = _cachedHeldItems[index];
                heldItem.gameObject.SetActive(true);
            }
            else
            {
                heldItem = null;
            }
            
            InitializeHeldItem();
            OnChangeHeldItem?.Invoke(heldItem);
        }
        
        void LoadHeldItemControllerAsync<T>(GameObject prefab, Action<T> onLoadCompleted = null) where T : Component
        {
            var go = Instantiate(prefab);
            if (go.TryGetComponent(out T controller))
            {
                onLoadCompleted?.Invoke(controller);
                return;
            }

            Destroy(go);
            var msg = $"Loaded prefab does not contain a component of type {typeof(T)}.";
            Game.Console.LogWarning(msg);
            Debug.LogWarning(msg);
        }
                
        public void EquipHotbarIndex(HotbarIndex index)
        {
            if (_isPlayingAnimation) return;
            if (currentlyEquippedIndex == index) return;
            if (!Player.Inventory.Hotbar.TryGetSlot((int) index, out var slot)) return;
            if (slot.IsEmpty) return;

            currentlyEquippedIndex = index;
            if (_cachedViewmodelsById.ContainsKey(slot.Item.Id))
            {
                EquipViewmodel(_cachedViewmodelsById[slot.Item.Id]);
            }
            HoldItemByIndex(index);
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

        void EquipViewmodel(Viewmodel viewmodel)
        {
            UnloadCurrentViewmodel();
            LoadViewmodel(viewmodel);

            armsController.PlayAnimation("equip");
            viewmodelAnimator?.CrossFade("equip", 0.1f, 0, 0f);
        }

        void UnloadCurrentViewmodel()
        {
            if (currentViewmodel != null && currentViewmodel.Model != null)
            {
                currentViewmodel?.Model.SetActive(false);
            }
        }

        void LoadViewmodel(Viewmodel viewmodel)
        {
            /// Load arms animations
            if (viewmodel.ArmsAnimations == null)
            {
                var msg = $"Item {viewmodel.ItemData.Id} has no arms animation.";
                Game.Console.Log(msg);
                Debug.LogWarning(msg);
            }
            armsController.SetAnimatorController(viewmodel.ArmsAnimations);

            /// Load viewmodel
            if (_cachedViewmodelsById.ContainsKey(viewmodel.ItemData.Id))
            {
                currentViewmodel = _cachedViewmodelsById[viewmodel.ItemData.Id];
                
                if (currentViewmodel.Model != null)
                {
                    currentViewmodel.Model.SetActive(true);
                                
                    if (currentViewmodel.Model.TryGetComponent(out Animator component))
                    {
                        viewmodelAnimator = component;
                    }
                    else
                    {
                        var msg = $"Item {currentViewmodel.ItemData.Id} has no Animator. No animations would be shown.";
                        Game.Console.Log(msg);
                        Debug.LogWarning(msg);
                    }

                    /// Attach Gun Muzzle Controller
                    if (currentViewmodel.Model.TryGetComponent(out FPPGunModel gunModel))
                    {
                        gunMuzzleController = gunModel.MuzzleController;
                    }
                }
                else
                {
                    var msg = $"Item {viewmodel.ItemData.Id} has no viewmodel.";
                    Game.Console.Log(msg);
                    Debug.LogWarning(msg);
                }
            }
        }

        void InitializeHeldItem()
        {
            if (heldItem is GunWeaponController gunWeapon)
            {
                Player.HUD.AmmoCounter.Initialize(gunWeapon);
            }
            
            InitializeHeldItemEvents();
        }

        void InitializeHeldItemEvents()
        {
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
                var reloadDuration = GetAnimationClipLength(viewmodelAnimator, "reload");
                reloadableWeapon.TryReload(reloadDuration);
            }
        }


        #region Event callbacks

        void OnPlayerMoveStateChanged(object sender, StateMachine<MoveStates>.StateChangedContext e)
        {
        }
        
        void OnPlayerActionStateChanged(object sender, StateMachine<ActionStates>.StateChangedContext e)
        {
            if (heldItem == null) return;
            if (_isPlayingAnimation) return;

            // if (heldItem != null)
            // {
            //     heldItem.SetStateFromAction(e.To);
            // }
        }

        void OnWeaponStateChanged(object sender, StateMachine<Enum>.StateChangedContext e)
        {
            if (heldItem == null) return;

            var animId = GetAnimIdFromState(e.To);
            if (!string.IsNullOrEmpty(animId))
            {
                armsController?.PlayAnimation(animId);
                viewmodelAnimator?.Play(animId, 0, 0f);

                var animLengthSeconds = GetAnimationClipLength(viewmodelAnimator, animId);
                StartCoroutine(FinishAnimation(animLengthSeconds));
            }
        }

        void OnMeleeWeaponStateChanged(object sender, StateMachine<MeleeWeaponStates>.StateChangedContext e)
        {
            if (heldItem == null) return;

            var animId = GetAnimIdFromState(e.To);
            if (!string.IsNullOrEmpty(animId))
            {
                armsController?.PlayAnimation(animId);
                viewmodelAnimator?.Play(animId, 0, 0f);

                var animLengthSeconds = GetAnimationClipLength(viewmodelAnimator, animId);
                StartCoroutine(FinishAnimation(animLengthSeconds));
            }
        }

        void OnRangedWeaponStateChanged(object sender, StateMachine<GunWeaponStates>.StateChangedContext e)
        {
            if (heldItem == null) return;

            var animId = GetAnimIdFromState(e.To);
            if (!string.IsNullOrEmpty(animId))
            {
                armsController?.PlayAnimation(animId);
                viewmodelAnimator?.Play(animId, 0, 0f);

                var animLengthSeconds = GetAnimationClipLength(viewmodelAnimator, animId);
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
            if (animator == null) return 0f;

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
