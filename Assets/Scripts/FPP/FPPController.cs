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
using MEC;

namespace UZSG.FPP
{
    /// <summary>
    /// Handles the functionalities of the Player's first-person perspective.
    /// </summary>
    public partial class FPPController : MonoBehaviour
    {
        Player player;
        public Player Player => player;
        [field: Space]

        public int SelectedHotbarIndex { get; protected set; }= 1;
        bool _isAnimationPlaying;
        string currentlyEquippedId;
        public string CurrentlyEquippedId => currentlyEquippedId;
        string lastEquippedId = "arms";
        Viewmodel currentViewmodel;
        public Viewmodel CurrentViewmodel => currentViewmodel;
        
        ItemData heldItemData;
        public ItemData HeldItem => heldItemData;
        public bool HasHeldItem => heldItemData != null;
        [SerializeField] FPPItemController fppItemController;
        bool _hasFppItemController = false;
        public FPPItemController FPPItemController => fppItemController;
        /// <summary>
        /// Key is the ItemData Id.
        /// </summary>
        Dictionary<string, FPPItemController> _cachedHeldItems = new();
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
            get => fppItemController != null && fppItemController.ItemData.Type == ItemType.Weapon;
        }
        public bool IsHoldingTool
        {
            get => fppItemController != null && fppItemController.ItemData.Type == ItemType.Tool;
        }
        public bool IsHoldingTile
        {
            get => fppItemController != null && fppItemController.ItemData.Type == ItemType.Tile;
        }
        bool _hasArmsAnimations;
        bool _hasViewmodelAnimations;
        bool _hasCameraAnimations;
        bool CanSwapEquipped
        {
            get => !_isAnimationPlaying && !IsPerforming;
        }

        #endregion


        #region Events

        /// <summary>
        /// Called everytime the FPP viewmodel is changed.
        /// </summary>
        public event Action<FPPItemController> OnFPPControllerChanged;
        public event Action<ItemData> OnHeldItemChanged;

        public event Action<int> OnSelectedHotbarChanged;

        //TODO: switch to state machine
        /// <summary>
        /// Called everytime the Player finishes an action (e.g., reload, fire, etc.)
        /// </summary>
        public event Action OnPerformFinished;

        #endregion


        [Header("Controllers")]
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
            player = GetComponent<Player>();
        }

        internal void Initialize()
        {
            InitializeEvents();
            InitializeInputs();
            cameraController.Initialize();
            // viewmodelController.Initialize();
            LoadAndEquipHands();
            SelectHotbarIndex(1);
            Game.UI.SetCursorVisible(false);
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

            SelectHotbarIndex(index);
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
                if (fppItemController.ItemData.Id == itemData.Id)
                {
                    EquipViewmodel(viewmodel); /// Equip on finish load :)
                }
            }
            
            return viewmodel;
        }

        void LoadFPPController(ItemData data, Action<FPPItemController> onLoadCompleted = null)
        {
            if (data is WeaponData weaponData)
            {
                if (weaponData.Category == WeaponCategory.Melee)
                {
                    LoadFPPItemControllerAsync<MeleeWeaponController>(meleeWeaponControllerPrefab, (meleeWeapon) =>
                    {
                        meleeWeapon.ItemData = data;
                        SetupFPPItemController(meleeWeapon);
                        onLoadCompleted?.Invoke(meleeWeapon);
                    });
                }
                else if (weaponData.Category == WeaponCategory.Ranged)
                {
                    LoadFPPItemControllerAsync<GunWeaponController>(gunWeaponControllerPrefab, (gunWeapon) =>
                    {
                        gunWeapon.ItemData = data;
                        SetupFPPItemController(gunWeapon);
                        onLoadCompleted?.Invoke(gunWeapon);
                    });
                }
                return;
            }
            else if (data is ToolData toolData)
            {
                LoadFPPItemControllerAsync<HeldToolController>(heldToolControllerPrefab, (tool) =>
                {
                    tool.ItemData = data;
                    SetupFPPItemController(tool);
                    onLoadCompleted?.Invoke(tool);
                });
                return;
            }

            if (data.Subtype == ItemSubtype.Consumable)
            {
                throw new NotImplementedException();
            }
        }

        void SetupFPPItemController(FPPItemController controller)
        {
            ItemData itemData = null;
            if (controller != null)
            {
                itemData = controller.ItemData;
                controller.transform.parent = heldItemsContainer.transform;
                controller.Owner = Player;
                controller.name = $"{itemData.DisplayName} (Held Item)";
                controller.Initialize();

                _cachedHeldItems[itemData.Id] = controller;
            }

            ReplaceActiveHeldItemGameObject(itemData);
            InitializeFPPController();
        }

        void ReplaceActiveHeldItemGameObject(ItemData itemData)
        {
            if (itemData == null) return;
            ReplaceActiveHeldItemGameObject(itemData.Id);
        }
        
        void ReplaceActiveHeldItemGameObject(string id)
        {
            if (string.IsNullOrEmpty(id)) return;

            if (fppItemController != null)
            {
                fppItemController.gameObject.SetActive(false);
            }

            if (_cachedHeldItems.ContainsKey(id))
            {
                fppItemController = _cachedHeldItems[id];
                fppItemController.gameObject.SetActive(true);
            }
            else
            {
                fppItemController = null;
            }
        }
        


        #region Public methods

        public void ShowBuildingHands()
        {
            EquipFPPController("arms");
            PlayAnimations("building");
        }
        
        public void SelectHotbarIndex(int index)
        {
            /// Reason being Equipment slots index involves 1 and 2
            /// and Hotbar slots involve 3 and above
            var slot = Player.Inventory.GetEquipmentOrHotbarSlot(index);
            if (slot == null)
            {
                Game.Console.LogWithUnity($"Tried to access Hotbar Slot {index}, but it's not available yet (wear a toolbelt or smth.)");
                return;
            }
            SelectedHotbarIndex = index;

            if (slot.HasItem)
            {
                HoldItem(slot.Item.Data);
            }
            else /// equipped to empty hotbar, so equip arms
            {
                HoldItem(null);
            }
            
            OnSelectedHotbarChanged?.Invoke(SelectedHotbarIndex);
        }

        /// <summary>
        /// Hold an item to be visible in FPP perspective. Does nothing if the item does not have a Viewmodel component.
        /// Passing a <c>null</c> ItemData equips "arms".
        /// </summary>
        public void HoldItem(ItemData itemData)
        {
            this.heldItemData = itemData;

            if (itemData == null)
            {
                EquipFPPController("arms");
                OnHeldItemChanged?.Invoke(Game.Items.GetData("arms"));
                return;
            }
            else
            {
                if (itemData.IsObject)
                {
                    ShowBuildingHands();
                    OnHeldItemChanged?.Invoke(heldItemData);
                }
                else if (itemData is IViewmodel) /// Item (probably) has a viewmodel component
                {
                    if (_cachedViewmodels.ContainsKey(itemData.Id)) /// viewmodel is already loaded
                    {
                        EquipFPPController(itemData.Id);
                        OnHeldItemChanged?.Invoke(heldItemData);
                        return;
                    }
                    else
                    {
                        LoadViewmodelAsset(itemData, equip: true);
                        LoadFPPController(itemData, onLoadCompleted: (loadedFppController) =>
                        {
                            EquipFPPController(itemData.Id);
                            OnHeldItemChanged?.Invoke(heldItemData);
                        });
                    }
                }
            }
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        public void EquipFPPController(string id)
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
                OnFPPControllerChanged?.Invoke(fppItemController);
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
                EquipFPPController(lastEquippedId);
            }
            else
            {
                lastEquippedId = currentlyEquippedId;
                EquipFPPController("arms");
            }
        }

        public void PerformReload()
        {
            if (_isAnimationPlaying) return;

            if (fppItemController is IReloadable reloadableWeapon)
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
                Game.Console.LogWithUnity(msg);
                return;
            }

            currentViewmodel = _cachedViewmodels[viewmodel.ItemData.Id];
            
            /// Setup arms animations
            armsController.SetAnimatorController(currentViewmodel.ArmsAnimations);
            if (viewmodel.ArmsAnimations != null)
            {
                _hasArmsAnimations = true;
            }
            else
            {
                _hasArmsAnimations = false;
                Game.Console.LogWithUnity($"Item '{currentViewmodel.ItemData.Id}' has no arms animation.");
            }
            
            if (currentViewmodel.Model != null)
            {
                currentViewmodel.Model.SetActive(true);
            }
            else
            {
                Game.Console.LogWithUnity($"Item '{currentViewmodel.ItemData.Id}' has no viewmodel.");
            }

            /// Setup model animations
            if (currentViewmodel.ModelAnimator != null)
            {
                _hasViewmodelAnimations = true;
                viewmodelAnimator = currentViewmodel.ModelAnimator;
            }
            else
            {
                _hasViewmodelAnimations = false;
                viewmodelAnimator = null;
                Game.Console.LogWithUnity($"Item '{currentViewmodel.ItemData.Id}' has no Model Animator. No animations would be shown.");
            }

            /// Setup camera animations
            if (currentViewmodel.CameraAnimator != null)
            {
                _hasCameraAnimations = true;
                cameraAnimator = currentViewmodel.CameraAnimator;
            }
            else
            {
                _hasCameraAnimations = false;
                cameraAnimator = null;
                Game.Console.LogWithUnity($"Item '{currentViewmodel.ItemData.Id}' has no Camera Animator. No animations would be shown.");
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

        void InitializeFPPController()
        {
            if (fppItemController is MeleeWeaponController meleeWeapon)
            {
                meleeWeapon.InitializeMeleeCollider(meleeWeaponCollider);
            }
            if (fppItemController is GunWeaponController gunWeapon)
            {
                Player.VitalsHUD.AmmoCounter.DisplayWeaponStats(gunWeapon);
            }
            
            InitializeFPPControllerEvents();
        }

        void InitializeFPPControllerEvents()
        {
            if (fppItemController is MeleeWeaponController meleeWeapon)
            {
                meleeWeapon.StateMachine.OnTransition -= OnMeleeWeaponStateChanged;
                
                meleeWeapon.StateMachine.OnTransition += OnMeleeWeaponStateChanged;
            }
            if (fppItemController is GunWeaponController rangedWeapon)
            {
                rangedWeapon.StateMachine.OnTransition -= OnRangedWeaponStateChanged;
                rangedWeapon.OnFire -= OnWeaponFired;

                rangedWeapon.StateMachine.OnTransition += OnRangedWeaponStateChanged;
                rangedWeapon.OnFire += OnWeaponFired;
            }
            if (fppItemController is HeldToolController tool)
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
            if (fppItemController == null) return;
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
                    HoldItem(null);
                }
            }
            else /// added Item to hotbar
            {
                if (IsCurrentlySelectedHotbar(e.ItemSlot.Index + 3)) /// + 3 cause the QUOTES HOTBAR QUOTES STARTS AT 3
                {
                    /// player has this slot equipped, immediately hold the new Item ig
                    HoldItem(e.NewItem.Data);
                }
            }
        }

        /// <summary>
        /// Checks if the given index is the currently selected hotbar index.
        /// </summary>
        bool IsCurrentlySelectedHotbar(int index)
        {
            return index == SelectedHotbarIndex;
        }

        // void OnWeaponStateChanged(StateMachine<Enum>.TransitionContext e)
        // {
        //     if (heldItem == null) return;

        //     var animId = GetAnimIdFromState(e.To);
        //     PlayAnimations(animId);
        // }

        void OnMeleeWeaponStateChanged(StateMachine<MeleeWeaponStates>.TransitionContext context)
        {
            if (fppItemController == null) return; /// TODO: optimiuze by subbing a flag instead

            if (context.To == MeleeWeaponStates.Attack)
            {
                var animId = GetAnimIdFromState(context.To);
                if (fppItemController is MeleeWeaponController meleeWeapon)
                {
                    PlayAnimations($"{animId}_{meleeWeapon.ComboCounter + 1}");
                }
            }
        }

        void OnRangedWeaponStateChanged(StateMachine<GunWeaponStates>.TransitionContext e)
        {
            if (fppItemController == null) return; /// TODO: optimiuze by subbing a flag instead
            
            var animId = GetAnimIdFromState(e.To);
            PlayAnimations(animId);
        }

        void OnToolWeaponStateChanged(StateMachine<ToolItemStates>.TransitionContext context)
        {
            if (fppItemController == null) return; /// TODO: optimiuze by subbing a flag instead

            if (context.To == ToolItemStates.Attack)
            {
                var animId = GetAnimIdFromState(context.To);
                if (fppItemController is HeldToolController tool)
                {
                    PlayAnimations($"{animId}_{tool.ComboCounter + 1}");
                }
            }
        }

        CoroutineHandle _fppAnimationCoroutineHandle;

        void PlayAnimations(string animId)
        {
            if (string.IsNullOrEmpty(animId)) return;

            if (_hasArmsAnimations)
            {
                armsController.PlayAnimation(animId);
            }
            if (_hasViewmodelAnimations)
            {
                viewmodelAnimator.Play(animId, 0, 0f);
            }
            if (_hasCameraAnimations)
            {
                cameraAnimator?.Play(animId, 0, 0f);
                cameraAnimationTarget.PlayAnimation();
            }

            /// viewmodelAnimator is used here because it's the one that
            /// usually has animations first :P idk tho
            var animLengthSeconds = GetAnimationClipLength(viewmodelAnimator, animId);
            Timing.KillCoroutines(_fppAnimationCoroutineHandle);
            _fppAnimationCoroutineHandle = Timing.RunCoroutine(FinishAnimation(animLengthSeconds));
        }
        
        void OnWeaponFired()
        {
            gunMuzzleController?.Fire();
            HandleWeaponRecoil();
        }

        #endregion

        void HandleWeaponRecoil()
        {
            if (fppItemController is GunWeaponController weapon)
            {
                var weaponData = weapon.WeaponData;
                var recoilInfo = weaponData.RangedAttributes.RecoilAttributes;

                cameraController.AddRecoilMotion(recoilInfo);
            }
        }
        
        void LoadFPPItemControllerAsync<T>(GameObject prefab, Action<T> onLoadCompleted = null) where T : Component
        {
            var go = Instantiate(prefab);
            if (go.TryGetComponent(out T controller))
            {
                onLoadCompleted?.Invoke(controller);
                onLoadCompleted = null;
                return;
            }

            Destroy(go);
            Game.Console.LogWithUnity($"Loaded prefab does not contain a component of type {typeof(T)}.");
        }

        IEnumerator<float> FinishAnimation(float durationSeconds)
        {
            if (_isAnimationPlaying) yield break;

            _isAnimationPlaying = true;
            IsPerforming = true;
            yield return Timing.WaitForSeconds(durationSeconds);
            _isAnimationPlaying = false;
            IsPerforming = false;
            cameraAnimationTarget.StopAnimation();
            OnPerformFinished?.Invoke();
            yield break;
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

        static string GetAnimIdFromState(Enum value)
        {
            return value.ToString().ToLower();
        }

        #endregion
    }
}
