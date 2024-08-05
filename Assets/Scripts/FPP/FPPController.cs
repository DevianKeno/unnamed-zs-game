using System;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using UZSG.Systems;
using UZSG.Data;
using UZSG.Entities;
using UZSG.Players;
using UZSG.Inventory;
using UZSG.Items;
using UZSG.Items.Weapons;
using UZSG.Items.Tools;

namespace UZSG.FPP
{
    /// <summary>
    /// Handles the functionalities of the Player's first-person perspective.
    /// </summary>
    public partial class FPPController : MonoBehaviour
    {
        public Player Player;
        [Space]
        
        bool _isEnabled;
        [SerializeField] HeldItemController heldItem;
        public HeldItemController HeldItem => heldItem;
        /// <summary>
        /// Key is the ItemData Id.
        /// </summary>
        Dictionary<string, HeldItemController> _cachedHeldItems = new();
        /// <summary>
        /// Key is the ItemData Id.
        /// </summary>
        Dictionary<string, Viewmodel> _cachedViewmodels = new();

        string currentlyEquippedId;
        public string CurrentlyEquippedIndex => currentlyEquippedId;
        string lastEquippedId;
        bool _isPlayingAnimation;
        Viewmodel currentViewmodel;
        public Viewmodel CurrentViewmodel => currentViewmodel;


        #region Events

        public event Action<HeldItemController> OnChangeHeldItem;
        public event Action OnPerformFinish;

        #endregion


        [Header("Controllers")]
        public bool AppendAnimationPrefixes = true;
        [SerializeField] FPPCameraInput cameraController;
        public FPPCameraInput CameraController => cameraController;
        [SerializeField] FPPArmsController armsController;
        [SerializeField] FPPViewmodelController viewmodelController;
        // [SerializeField] FPPAimDownSights adsController;

        Animator viewmodelAnimator;
        Animator cameraAnimator;
        [SerializeField] FPPCameraAnimationTarget cameraAnimationTarget;
        [SerializeField] GunMuzzleController gunMuzzleController;
        [SerializeField] GameObject heldItemsContainer;
        
        [Header("Held Item Controller Prefabs")]
        [SerializeField] GameObject meleeWeaponControllerPrefab;
        [SerializeField] GameObject gunWeaponControllerPrefab;
        [SerializeField] GameObject heldToolControllerPrefab;

        bool _hasHeldItem;
        bool _isPerforming;
        /// <summary>
        /// Whether if the FPP is performing any actions.
        /// </summary>
        public bool IsPerforming => _isPerforming;
        bool _hasArmsAnimations;
        bool _hasViewmodelAnimations;
        bool _hasCameraAnimations;
        
        void Awake()
        {
            Player = GetComponent<Player>();
        }

        internal void Initialize()
        {
            InitializeEvents();
            cameraController.Initialize();
            // viewmodelController.Initialize();
            LoadAndEquipHands();
            Game.UI.ToggleCursor(false);
        }

        void InitializeEvents()
        {
            Player.MoveStateMachine.OnStateChanged += OnPlayerMoveStateChanged;
            Player.ActionStateMachine.OnStateChanged += OnPlayerActionStateChanged;
        }

        IEnumerator StartPerform(float duration = 0.25f)
        {
            _isPerforming = true;
            yield return new WaitForSeconds(duration);
            _isPerforming = false;
        }

        void LoadAndEquipHands()
        {
            var armsData = Game.Items.GetData("arms");
            HoldItem(armsData);
        }


        #region Public

        public void ToggleControls(bool enabled)
        {
            cameraController.ToggleControls(enabled);
        }

        /// <summary>
        /// Hold an item in FPP perspective. Does nothing if the item is not holdable.
        /// </summary>
        public void HoldItem(ItemData data)
        {
            StartCoroutine(StartPerform(1f));
            LoadViewmodelAsset(data, equip: true);
            LoadHeldItem(data, () =>
            {
                EquipHeldItem(data.Id);
            });
        }

        #endregion

        
        /// <summary>
        /// Load view model from addressables.
        /// </summary>
        async void LoadViewmodelAsset(ItemData data, bool equip = true)
        {
            if (data is not IViewmodel viewmodel) return;
            
            await LoadViewmodelAssetAsync(viewmodel, equip);
        }

        async Task<Viewmodel> LoadViewmodelAssetAsync(IViewmodel item, bool equip)
        {
            var itemData = item as ItemData;
            if (_cachedViewmodels.ContainsKey(itemData.Id)) return null;
            
            var viewmodel = await viewmodelController.LoadViewmodelAssetAsync(item);
            if (viewmodel != null) /// Cache loaded viewmodel
            {
                _cachedViewmodels[itemData.Id] = viewmodel;
                
                if (equip)
                {
                    /// Check if for some reason had switched
                    if (heldItem.ItemData.Id != itemData.Id) return viewmodel;

                    /// Equip viewmodel on finish load :)
                    EquipViewmodel(viewmodel);
                }
            }
            return viewmodel;
        }

        void LoadHeldItem(ItemData data, Action onDoneInitialize = null)
        {
            if (data is not IViewmodel viewmodel) return;

            if (data is WeaponData weaponData)
            {
                if (weaponData.Category == WeaponCategory.Melee)
                {
                    LoadHeldItemControllerAsync<MeleeWeaponController>(meleeWeaponControllerPrefab, (meleeWeapon) =>
                    {
                        InitializeHeldItemController(data, meleeWeapon);
                        onDoneInitialize?.Invoke();
                    });
                }
                else if (weaponData.Category == WeaponCategory.Ranged)
                {
                    LoadHeldItemControllerAsync<GunWeaponController>(gunWeaponControllerPrefab, (gunWeapon) =>
                    {
                        InitializeHeldItemController(data, gunWeapon);
                        onDoneInitialize?.Invoke();
                    });
                }
                return;
            }
            else if (data is ToolData toolData)
            {
                LoadHeldItemControllerAsync<HeldToolController>(heldToolControllerPrefab, (tool) =>
                {
                    InitializeHeldItemController(data, tool);
                    onDoneInitialize?.Invoke();
                });
                return;
            }

            if (data.Subtype == ItemSubtype.Consumable)
            {
                throw new NotImplementedException();
            }
        }

        void InitializeHeldItemController(ItemData data ,HeldItemController controller)
        {
            controller.transform.parent = heldItemsContainer.transform;
            controller.ItemData = data;
            controller.Owner = Player;
            controller.Initialize();
            _cachedHeldItems[data.Id] = controller;
            controller.name = $"{data.Name} (Held Item)";

            HoldItemById(data.Id);
            InitializeHeldItem();
        }

        void HoldItemById(string id)
        {
            if (heldItem != null)
            {
                heldItem.gameObject.SetActive(false);
            }

            if (_cachedHeldItems.ContainsKey(id))
            {
                heldItem = _cachedHeldItems[id];
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
                
        public void EquipHeldItem(string id)
        {
            if (string.IsNullOrEmpty(id)) return;
            if (_isPlayingAnimation) return;
            if (currentlyEquippedId == id) return;

            if (_cachedHeldItems.ContainsKey(id))
            {
                UnloadCurrentViewmodel();
                currentlyEquippedId = id;
                if (_cachedViewmodels.ContainsKey(id))
                {
                    EquipViewmodel(_cachedViewmodels[id]);
                }
                HoldItemById(id);
            }
        }

        public void Unholster()
        {
            if (_isPlayingAnimation) return;

            /// Swaps
            if (currentlyEquippedId == "arms")
            {
                EquipHeldItem(lastEquippedId);
            }
            else
            {
                lastEquippedId = currentlyEquippedId;
                EquipHeldItem("arms");
            }
        }

        void EquipViewmodel(Viewmodel viewmodel)
        {
            LoadViewmodel(viewmodel);

            PlayAnimations("equip");
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
            /// Load cached viewmodel
            if (!_cachedViewmodels.ContainsKey(viewmodel.ItemData.Id))
            {
                var msg = $"Tried to load held item '{viewmodel.ItemData.Id}' but it's not loaded nor equipped?";
                Game.Console.LogAndUnityLog(msg);
                return;
            }

            currentViewmodel = _cachedViewmodels[viewmodel.ItemData.Id];
            
            /// Load arms animations
            armsController.SetAnimatorController(currentViewmodel.ArmsAnimations);
            if (viewmodel.ArmsAnimations != null)
            {
                _hasArmsAnimations = true;
            }
            else
            {
                _hasArmsAnimations = false;
                var msg = $"Item '{currentViewmodel.ItemData.Id}' has no arms animation.";
                Game.Console.LogAndUnityLog(msg);
            }
            
            if (currentViewmodel.Model != null)
            {
                currentViewmodel.Model.SetActive(true);
            }
            else
            {
                var msg = $"Item '{currentViewmodel.ItemData.Id}' has no viewmodel.";
                Game.Console.LogAndUnityLog(msg);
            }

            /// Load model animations
            if (currentViewmodel.ModelAnimator != null)
            {
                _hasViewmodelAnimations = true;
                viewmodelAnimator = currentViewmodel.ModelAnimator;
            }
            else
            {
                _hasViewmodelAnimations = false;
                viewmodelAnimator = null;
                var msg = $"Item '{currentViewmodel.ItemData.Id}' has no Model Animator. No animations would be shown.";
                Game.Console.LogAndUnityLog(msg);
            }

            /// Load camera animations
            if (currentViewmodel.CameraAnimator != null)
            {
                _hasCameraAnimations = true;
                cameraAnimator = currentViewmodel.CameraAnimator;
            }
            else
            {
                _hasCameraAnimations = false;
                cameraAnimator = null;
                var msg = $"Item '{currentViewmodel.ItemData.Id}' has no Camera Animator. No animations would be shown.";
                Game.Console.LogAndUnityLog(msg);
            }

            if (currentViewmodel.CameraAnimationSource != null)
            {
                cameraAnimationTarget.Source = currentViewmodel.CameraAnimationSource;
            }
            else
            {
                cameraAnimationTarget.Source = null;
            }

            /// This should not be here
            /// fuck man this gonna be hard
            // if (currentViewmodel.Model.TryGetComponent(out FPPAimDownSights ads))
            // {
            //     adsController = ads;
            //     adsController.FPPCamera = cameraController.transform;
            // }
            // else
            // {
            //     adsController = null;
            // }

            /// This should not be here
            /// Attach Gun Muzzle Controller
            if (currentViewmodel.Model.TryGetComponent(out FPPGunModel gunModel))
            {
                gunMuzzleController = gunModel.MuzzleController;
            }
            else
            {
                gunMuzzleController = null;
            }

            return;
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
            else if (heldItem is HeldToolController tool)
            {
                tool.StateMachine.OnStateChanged -= OnToolWeaponStateChanged;
                
                tool.StateMachine.OnStateChanged += OnToolWeaponStateChanged;
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
            if (_isPerforming) return;
            if (_isPlayingAnimation) return;

            if (e.To == ActionStates.Secondary)
            {
                // adsController?.AimDownSights();
            }

            // if (heldItem != null)
            // {
            //     heldItem.SetStateFromAction(e.To);
            // }
        }

        void OnWeaponStateChanged(object sender, StateMachine<Enum>.StateChangedContext e)
        {
            if (heldItem == null) return;

            var animId = GetAnimIdFromState(e.To);
            PlayAnimations(animId);
        }

        void OnMeleeWeaponStateChanged(object sender, StateMachine<MeleeWeaponStates>.StateChangedContext e)
        {
            if (heldItem == null) return;

            var animId = GetAnimIdFromState(e.To);
            PlayAnimations(animId);
        }

        void OnRangedWeaponStateChanged(object sender, StateMachine<GunWeaponStates>.StateChangedContext e)
        {
            if (heldItem == null) return;
            
            var animId = GetAnimIdFromState(e.To);
            PlayAnimations(animId);
        }

        void OnToolWeaponStateChanged(object sender, StateMachine<ToolItemStates>.StateChangedContext e)
        {
            if (heldItem == null) return;

            var animId = GetAnimIdFromState(e.To);
            PlayAnimations(animId);
        }

        void PlayAnimations(string animId)
        {
            if (string.IsNullOrEmpty(animId)) return;
            
            string armsAnim = AppendAnimationPrefixes ? $"a_{animId}" : animId;
            string viewmodelAnim = AppendAnimationPrefixes ? $"m_{animId}" : animId;
            string cameraAnim = AppendAnimationPrefixes ? $"c_{animId}" : animId;

            if (_hasArmsAnimations) armsController.PlayAnimation(armsAnim);
            if (_hasViewmodelAnimations) viewmodelAnimator.Play(viewmodelAnim, 0, 0f);
            if (_hasCameraAnimations)
            {
                cameraAnimator?.Play(cameraAnim, 0, 0f);
                cameraAnimationTarget.Enable();
            }

            /// viewmodelAnimator is used here because it's the one that
            /// usually has animations first :P idk tho
            var animLengthSeconds = GetAnimationClipLength(viewmodelAnimator, viewmodelAnim);
            StopAllCoroutines();
            StartCoroutine(FinishAnimation(animLengthSeconds));
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
            if (_isPlayingAnimation) yield return null;
            _isPlayingAnimation = true;
            _isPerforming = true;

            yield return new WaitForSeconds(durationSeconds);
            _isPlayingAnimation = false;
            _isPerforming = false;
            cameraAnimationTarget.Disable();
            OnPerformFinish?.Invoke();
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
