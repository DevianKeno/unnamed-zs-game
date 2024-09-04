using System;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using UZSG.Systems;
using UZSG.Data;
using UZSG.Entities;
using UZSG.Players;
using UZSG.Items;
using UZSG.Items.Weapons;
using UZSG.Items.Tools;
using UZSG.Attacks;
using UZSG.Inventory;
using UnityEngine.InputSystem;

namespace UZSG.FPP
{
    /// <summary>
    /// Handles the functionalities of the Player's first-person perspective.
    /// </summary>
    public partial class FPPController : MonoBehaviour
    {
        public Player Player;
        [Space]

        int _selectedHotbarSlot = 1;
        bool _isAnimationPlaying;
        string currentlyEquippedId;
        public string CurrentlyEquippedId => currentlyEquippedId;
        string lastEquippedId = "arms";
        Viewmodel currentViewmodel;
        public Viewmodel CurrentViewmodel => currentViewmodel;
        
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


        #region Properties

        public bool IsBusy { get; private set; }
        /// <summary>
        /// Whether if the FPP is performing any actions.
        /// </summary>
        public bool IsPerforming { get; private set; }
        public bool IsHoldingWeapon
        {
            get => heldItem != null && heldItem.ItemData.Type == ItemType.Weapon;
        }
        public bool IsHoldingTool
        {
            get => heldItem != null && heldItem.ItemData.Type == ItemType.Tool;
        }
        public bool HasArmsAnimations { get; private set; }
        public bool HasViewmodelAnimations { get; private set; }
        public bool HasCameraAnimations { get; private set; }
        public bool CanSwapEquipped
        {
            get => !_isAnimationPlaying && !IsPerforming;
        }

        #endregion


        #region Events

        /// <summary>
        /// HeldItemController is the newly Held Item.
        /// </summary>
        public event Action<HeldItemController> OnChangeHeldItem;

        //TODO: switch to state machine
        /// <summary>
        /// Called everytime the Player finishes an action (e.g., reload, fire, etc.)
        /// </summary>
        public event Action OnPerformFinish;

        #endregion


        [Header("Controllers")]
        public bool AppendArmaturePrefix = true;
        public char Prefix;
        public bool AppendAnimationPrefixes = true;

        [SerializeField] FPPCameraInput cameraController;
        public FPPCameraInput Camera => cameraController;
        [SerializeField] FPPArmsController armsController;
        [SerializeField] FPPViewmodelController viewmodelController;
        [SerializeField] FPPRecoilCamera recoilCamera;
        // [SerializeField] FPPAimDownSights adsController;

        Animator viewmodelAnimator;
        Animator cameraAnimator;
        [SerializeField] FPPCameraAnimationTarget cameraAnimationTarget;
        [SerializeField] GunMuzzleController gunMuzzleController;
        [SerializeField] MeleeWeaponCollider meleeWeaponCollider;
        [SerializeField] GameObject heldItemsContainer;
        
        [Header("Held Item Controller Prefabs")]
        [SerializeField] GameObject meleeWeaponControllerPrefab;
        [SerializeField] GameObject gunWeaponControllerPrefab;
        [SerializeField] GameObject heldToolControllerPrefab;

        Dictionary<string, InputAction> inputs = new();


        #region Initializing methods
        
        void Awake()
        {
            Player = GetComponent<Player>();
        }

        internal void Initialize()
        {
            InitializeEvents();
            InitializeInputs();
            cameraController.Initialize();
            // viewmodelController.Initialize();
            LoadAndEquipHands();
            Game.UI.ToggleCursor(false);
        }

        void InitializeEvents()
        {
            Player.MoveStateMachine.OnTransition += OnMoveTransition;
            Player.ActionStateMachine.OnTransition += OnActionTransition;
            Player.Inventory.Hotbar.OnSlotItemChanged += OnHotbarItemChanged;
        }

        void InitializeInputs()
        {
            inputs["Hotbar"] = Game.Main.GetInputAction("Hotbar", "Player Actions");
            inputs["Hotbar"].performed += OnInputHotbar;
            
            inputs["Reload"] = Game.Main.GetInputAction("Reload", "Player Actions");
            inputs["Reload"].performed += OnPerformReload;

            inputs["Unholster"] = Game.Main.GetInputAction("Unholster", "Player Actions");
            inputs["Unholster"].performed += OnUnholster;
            
            inputs["Back"] = Game.Main.GetInputAction("Back", "Global");
        }

        #endregion

        
        #region Player input callbacks

        void OnInputHotbar(InputAction.CallbackContext context)
        {
            if (!int.TryParse(context.control.displayName, out int index)) return;

            var slot = Player.Inventory.GetEquipmentOrHotbarSlot(index);
            if (slot == null)
            {
                Game.Console.LogAndUnityLog($"Tried to access Hotbar Slot {index}, but it's not available yet (wear a toolbelt or smth.)");
                return;
            }

            _selectedHotbarSlot = index;

            if (slot.HasItem)
            {
                EquipHeldItem(slot.Item.Id);
            }
            else /// empty hotbar just equips arms
            {
                EquipHeldItem("arms");
            }
        }

        void OnPerformReload(InputAction.CallbackContext context)
        {
            PerformReload();
        }

        void OnUnholster(InputAction.CallbackContext context)
        {
            Unholster();
        }

        #endregion


        void LoadAndEquipHands()
        {
            var armsData = Game.Items.GetData("arms");
            HoldItem(armsData);
        }

        IEnumerator StartTimedAction(float duration = 0.25f)
        {
            IsPerforming = true;
            yield return new WaitForSeconds(duration);
            IsPerforming = false;
        }

        /// <summary>
        /// Load viewmodel from addressables.
        /// </summary>
        async void LoadViewmodelAsset(ItemData data, bool equip = true)
        {
            await LoadViewmodelAssetAsync(data as IViewmodel, equip);
        }

        async void UnloadViewmodelAsset(ItemData data)
        {
            // await null;
        }

        async Task<Viewmodel> LoadViewmodelAssetAsync(IViewmodel item, bool equip)
        {
            var itemData = item as ItemData;
            var viewmodel = await viewmodelController.LoadViewmodelAssetAsync(item);
            if (viewmodel == null) return null;
            
            /// Cache loaded viewmodel
            _cachedViewmodels[itemData.Id] = viewmodel;
            if (equip)
            {
                /// If the Player is still holding the same Item, equip,
                /// but what reason or how would the currently equipped be changed—it should not be
                if (heldItem.ItemData.Id == itemData.Id)
                {
                    EquipViewmodel(viewmodel); /// Equip on finish load :)
                }
            }
            
            return viewmodel;
        }

        void LoadHeldItem(ItemData data, Action<HeldItemController> onDoneInitialize = null)
        {
            if (data is WeaponData weaponData)
            {
                if (weaponData.Category == WeaponCategory.Melee)
                {
                    LoadHeldItemControllerAsync<MeleeWeaponController>(meleeWeaponControllerPrefab, (meleeWeapon) =>
                    {
                        meleeWeapon.ItemData = data;
                        onDoneInitialize?.Invoke(meleeWeapon);
                    });
                }
                else if (weaponData.Category == WeaponCategory.Ranged)
                {
                    LoadHeldItemControllerAsync<GunWeaponController>(gunWeaponControllerPrefab, (gunWeapon) =>
                    {
                        gunWeapon.ItemData = data;
                        onDoneInitialize?.Invoke(gunWeapon);
                    });
                }
                return;
            }
            else if (data is ToolData toolData)
            {
                LoadHeldItemControllerAsync<HeldToolController>(heldToolControllerPrefab, (tool) =>
                {
                    tool.ItemData = data;
                    onDoneInitialize?.Invoke(tool);
                });
                return;
            }

            if (data.Subtype == ItemSubtype.Consumable)
            {
                throw new NotImplementedException();
            }
        }

        void SetupHeldItemController(HeldItemController controller)
        {
            var itemData = controller.ItemData;
            controller.transform.parent = heldItemsContainer.transform;
            controller.Owner = Player;
            controller.name = $"{itemData.Name} (Held Item)";
            controller.Initialize();

            _cachedHeldItems[itemData.Id] = controller;

            ReplaceActiveHeldItemGameObject(itemData.Id);
            InitializeHeldItem();
        }

        /// <summary>
        /// Setup Held Item for the different FPP controller components.
        /// </summary>
        void ReplaceActiveHeldItemGameObject(string id)
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
        }
        


        #region Public methods

        /// <summary>
        /// Hold an item in FPP perspective. Does nothing if the item is not holdable.
        /// </summary>
        public void HoldItem(ItemData data)
        {
            if (data is not IViewmodel viewmodel)
            {
                Game.Console.Log($"Item '{data.Id}' does not have a viewmodel asset");
                return;
            }

            if (_cachedViewmodels.ContainsKey(data.Id)) /// viewmodel is already loaded
            {
                EquipHeldItem(data.Id);
                return;
            };

            StartCoroutine(StartTimedAction(1f));/// TODO: subject to change
            LoadViewmodelAsset(data, equip: true);
            LoadHeldItem(data, (heldItem) =>
            {
                SetupHeldItemController(heldItem);
                EquipHeldItem(data.Id);
            });
        }

        /// <summary>
        /// Releases the viewmodel asset via Addressables.
        /// </summary>
        public void ReleaseItem(ItemData data)
        {
            if (!_cachedViewmodels.TryGetValue(data.Id, out var viewmodel)) return;

            Destroy(viewmodel.Model.gameObject); /// salt
            _cachedViewmodels.Remove(data.Id);
            
            // var viewmodel = _cachedViewmodels[data.Id];
            UnloadViewmodelAsset(data);
        }

        public void EquipHeldItem(string id)
        {
            if (string.IsNullOrEmpty(id)) return;
            if (_isAnimationPlaying) return;
            if (currentlyEquippedId == id) return;

            if (_cachedHeldItems.ContainsKey(id))
            {
                UnloadCurrentViewmodel();
                currentlyEquippedId = id;
                if (_cachedViewmodels.ContainsKey(id))
                {
                    EquipViewmodel(_cachedViewmodels[id]);
                }
                ReplaceActiveHeldItemGameObject(id);
                OnChangeHeldItem?.Invoke(heldItem);
            }
        }

        /// <summary>
        /// Unholsters the current equipped.
        /// If from arms, switch back to last equipped.
        /// </summary>
        public void Unholster()
        {
            if (_isAnimationPlaying) return;

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

        public void PerformReload()
        {
            if (_isAnimationPlaying) return;

            if (heldItem is IReloadable reloadableWeapon)
            {
                var reloadDuration = GetAnimationClipLength(viewmodelAnimator, "reload");
                reloadableWeapon.TryReload(reloadDuration);
            }
        }

        public void ToggleControls(bool enabled)
        {
            cameraController.ToggleControls(enabled);
        }

        #endregion

        
        void EquipViewmodel(Viewmodel viewmodel)
        {
            SetupViewmodelComponents(viewmodel);
            PlayAnimations("equip");
        }

        void UnloadCurrentViewmodel()
        {
            if (currentViewmodel != null && currentViewmodel.Model != null)
            {
                currentViewmodel?.Model.SetActive(false);
            }
        }

        /// <summary>
        /// Setup the different controller components of the viewmodel for the FPP Controller.
        /// </summary>
        void SetupViewmodelComponents(Viewmodel viewmodel)
        {
            /// Validate viewmodel if is cached/loaded
            if (!_cachedViewmodels.ContainsKey(viewmodel.ItemData.Id))
            {
                var msg = $"Tried to setup Held Item '{viewmodel.ItemData.Id}' but it's not loaded nor equipped?";
                Game.Console.LogAndUnityLog(msg);
                return;
            }

            currentViewmodel = _cachedViewmodels[viewmodel.ItemData.Id];
            
            /// Setup arms animations
            armsController.SetAnimatorController(currentViewmodel.ArmsAnimations);
            if (viewmodel.ArmsAnimations != null)
            {
                HasArmsAnimations = true;
            }
            else
            {
                HasArmsAnimations = false;
                Game.Console.LogAndUnityLog($"Item '{currentViewmodel.ItemData.Id}' has no arms animation.");
            }
            
            if (currentViewmodel.Model != null)
            {
                currentViewmodel.Model.SetActive(true);
            }
            else
            {
                Game.Console.LogAndUnityLog($"Item '{currentViewmodel.ItemData.Id}' has no viewmodel.");
            }

            /// Setup model animations
            if (currentViewmodel.ModelAnimator != null)
            {
                HasViewmodelAnimations = true;
                viewmodelAnimator = currentViewmodel.ModelAnimator;
            }
            else
            {
                HasViewmodelAnimations = false;
                viewmodelAnimator = null;
                Game.Console.LogAndUnityLog($"Item '{currentViewmodel.ItemData.Id}' has no Model Animator. No animations would be shown.");
            }

            /// Setup camera animations
            if (currentViewmodel.CameraAnimator != null)
            {
                HasCameraAnimations = true;
                cameraAnimator = currentViewmodel.CameraAnimator;
            }
            else
            {
                HasCameraAnimations = false;
                cameraAnimator = null;
                Game.Console.LogAndUnityLog($"Item '{currentViewmodel.ItemData.Id}' has no Camera Animator. No animations would be shown.");
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

            /// This should not be here
            /// Attach meleeWeaponCollider
            if (currentViewmodel.Model.TryGetComponent(out MeleeWeaponCollider meleeCollider))
            {
                meleeWeaponCollider = meleeCollider;
            }
            else
            {
                meleeWeaponCollider = null;
            }

            armsController.SetViewmodelSettings(viewmodel.Settings);
            viewmodelController.SetViewmodelSettings(viewmodel.Settings);

            return;
        }

        void InitializeHeldItem()
        {
            if (heldItem is GunWeaponController gunWeapon)
            {
                Player.VitalsHUD.AmmoCounter.DisplayWeaponStats(gunWeapon);
            }
            
            InitializeHeldItemEvents();
        }

        void InitializeHeldItemEvents()
        {
            if (heldItem is MeleeWeaponController meleeWeapon)
            {
                meleeWeapon.SetMeleeCollider(meleeWeaponCollider);
                meleeWeapon.StateMachine.OnTransition -= OnMeleeWeaponStateChanged;
                
                meleeWeapon.StateMachine.OnTransition += OnMeleeWeaponStateChanged;
            }
            else if (heldItem is GunWeaponController rangedWeapon)
            {
                rangedWeapon.StateMachine.OnTransition -= OnRangedWeaponStateChanged;
                rangedWeapon.OnFire -= OnWeaponFired;

                rangedWeapon.StateMachine.OnTransition += OnRangedWeaponStateChanged;
                rangedWeapon.OnFire += OnWeaponFired;
            }
            else if (heldItem is HeldToolController tool)
            {
                tool.StateMachine.OnTransition -= OnToolWeaponStateChanged;
                
                tool.StateMachine.OnTransition += OnToolWeaponStateChanged;
            }
        }


        #region Event callbacks

        void OnMoveTransition(StateMachine<MoveStates>.TransitionContext t)
        {

        }
        
        void OnActionTransition(StateMachine<ActionStates>.TransitionContext t)
        {
            if (heldItem == null) return;
            if (IsPerforming) return;
            if (_isAnimationPlaying) return;

            if (t.To == ActionStates.Secondary)
            {
                // adsController?.AimDownSights();
            }

            // if (heldItem != null)
            // {
            //     heldItem.SetStateFromAction(e.To);
            // }
        }

        void OnHotbarItemChanged(object sender, ItemSlot.ItemChangedContext e)
        {
            if (e.NewItem == Item.None) /// removed Item from hotbar
            {
                /// check if other Hotbar slots still contains the old item
                if (!Player.Inventory.Hotbar.Contains(e.OldItem))
                {
                    ReleaseItem(e.OldItem.Data);
                }
            }
            else /// added Item to hotbar
            {
                if (e.ItemSlot.Index + 3 == _selectedHotbarSlot) /// + 3 cause the QUOTES HOTBAR QUOTES STARTS AT 3
                {
                    /// player has this slot equipped, immediately hold the new Item ig
                    HoldItem(e.NewItem.Data);
                }
            }
        }

        // void OnWeaponStateChanged(StateMachine<Enum>.TransitionContext e)
        // {
        //     if (heldItem == null) return;

        //     var animId = GetAnimIdFromState(e.To);
        //     PlayAnimations(animId);
        // }

        void OnMeleeWeaponStateChanged(StateMachine<MeleeWeaponStates>.TransitionContext e)
        {
            if (heldItem == null) return;

            var animId = GetAnimIdFromState(e.To);
            PlayAnimations(animId);
        }

        void OnRangedWeaponStateChanged(StateMachine<GunWeaponStates>.TransitionContext e)
        {
            if (heldItem == null) return;
            
            var animId = GetAnimIdFromState(e.To);
            PlayAnimations(animId);
        }

        void OnToolWeaponStateChanged(StateMachine<ToolItemStates>.TransitionContext e)
        {
            if (heldItem == null) return;

            var animId = GetAnimIdFromState(e.To);
            PlayAnimations(animId);
        }

        void PlayAnimations(string animId)
        {
            if (string.IsNullOrEmpty(animId)) return;

            animId = AppendArmaturePrefix ? $"{Prefix}{animId}" : animId;
            
            string armsAnim = AppendAnimationPrefixes ? $"a_{animId}" : animId;
            string viewmodelAnim = AppendAnimationPrefixes ? $"m_{animId}" : animId;
            string cameraAnim = AppendAnimationPrefixes ? $"c_{animId}" : animId;

            if (HasArmsAnimations) armsController.PlayAnimation(armsAnim);
            if (HasViewmodelAnimations) viewmodelAnimator.Play(viewmodelAnim, 0, 0f);
            if (HasCameraAnimations)
            {
                cameraAnimator?.Play(cameraAnim, 0, 0f);
                cameraAnimationTarget.PlayAnimation();
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
        }

        #endregion

        void HandleWeaponRecoil()
        {
            if (heldItem is GunWeaponController weapon)
            {
                var weaponData = weapon.WeaponData;
                var recoilInfo = weaponData.RangedAttributes.RecoilAttributes;

                cameraController.AddRecoilMotion(recoilInfo);
            }
        }
        
        void LoadHeldItemControllerAsync<T>(GameObject prefab, Action<T> onLoadCompleted = null) where T : Component
        {
            var go = Instantiate(prefab);
            if (go.TryGetComponent(out T controller))
            {
                onLoadCompleted?.Invoke(controller);
                onLoadCompleted = null;
                return;
            }

            Destroy(go);
            Game.Console.LogAndUnityLog($"Loaded prefab does not contain a component of type {typeof(T)}.");
        }

        IEnumerator FinishAnimation(float durationSeconds)
        {
            if (_isAnimationPlaying) yield return null;
            _isAnimationPlaying = true;
            IsPerforming = true;

            yield return new WaitForSeconds(durationSeconds);
            _isAnimationPlaying = false;
            IsPerforming = false;
            cameraAnimationTarget.StopAnimation();
            OnPerformFinish?.Invoke();
            yield return null;
        }


        #region Helper functions

        float GetAnimationClipLength(Animator animator, string name)
        {
            #region TODO:change this to a flag
            #endregion
            if (animator == null || animator.runtimeAnimatorController == null) return 0f;

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
