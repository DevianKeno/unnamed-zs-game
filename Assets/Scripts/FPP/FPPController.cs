using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.InputSystem;
using MEC;

using UZSG.Data;
using UZSG.Entities;
using UZSG.Inventory;
using UZSG.Items;
using UZSG.Items.Weapons;
using UZSG.Items.Tools;
using UZSG.Players;

namespace UZSG.FPP
{
    /// <summary>
    /// Handles the functionalities of the Player's first-person perspective.
    /// </summary>
    public partial class FPPController : MonoBehaviour
    {
        [SerializeField] protected Player player;
        public Player Player => player;

        [field: Space]
        bool _isAnimationPlaying;
        bool _hasArmsAnimations;
        bool _hasViewmodelAnimations;
        bool _hasCameraAnimations;
        bool _isWaitingForHotbarSwitch;
        string _lastEquippedItemId = "arms";
        string _currentEquippedItemId = "arms";
        ItemData currentHeldItemData = null;
        ItemData armsItemData;

        [SerializeField] ViewmodelComponent currentViewmodelComponent = null;
        public ViewmodelComponent CurrentViewmodel => currentViewmodelComponent;
        
        [SerializeField] FPPItemController currentFPPItemController = null;
        public FPPItemController CurrentFPPItem => currentFPPItemController;

        /// <summary>
        /// <c>string</c> is the Id of the item.
        /// </summary>
        Dictionary<string, ViewmodelComponent> loadedViewmodelComponents = new();
        /// <summary>
        /// Key is the the hotbar Index.
        /// </summary>
        Dictionary<int, FPPItemController> loadedFPPItems = new();
        Dictionary<string, InputAction> inputs = new();


        #region Properties

        /// <summary>
        /// Whether if the player is performing any actions.
        /// </summary>
        public bool IsBusy
        {
            get => _isAnimationPlaying;
        }
        public bool IsHoldingWeapon
        {
            get => currentFPPItemController != null && currentFPPItemController.ItemData.Type == ItemType.Weapon;
        }
        public bool IsHoldingTool
        {
            get => currentFPPItemController != null && currentFPPItemController.ItemData.Type == ItemType.Tool;
        }
        public bool IsHoldingTile
        {
            get => currentFPPItemController != null && currentFPPItemController.ItemData.Type == ItemType.Object;
        }
        public bool CanSwapEquipped
        {
            get => !_isAnimationPlaying && !IsBusy;
        }
        public int SelectedHotbarIndex { get; protected set; } = 1;

        #endregion


        #region Events

        /// <summary>
        /// Raised everytime the FPP viewmodel is changed.
        /// </summary>
        public event Action<FPPItemController> OnFPPItemChanged;
        public event Action<ItemData> OnHeldItemChanged;
        public event Action<int> OnSelectedHotbarIndexChanged;

        //TODO: switch to state machine
        /// <summary>
        /// Raised everytime the Player finishes an action (e.g., reload, fire, etc.)
        /// </summary>
        public event Action OnPerformFinished;

        #endregion


        [Header("Controllers")]
        [SerializeField] FPPCameraInput cameraController;
        public FPPCameraInput Camera => cameraController;
        [SerializeField] FPPArmsController fppArmsController;
        [SerializeField] FPPViewmodelController viewmodelController;
        // [SerializeField] FPPAimDownSights adsController;
        Animator runtimeViewmodelAnimator; /// changed in runtime
        Animator runtimeFPPCameraAnimator; /// changed in runtime
        [SerializeField] FPPCameraAnimationTarget cameraAnimationTarget;
        [SerializeField] GameObject heldItemsContainer;
        
        [Header("Held Item Controller Prefabs")]
        [SerializeField] GameObject meleeWeaponControllerPrefab;
        [SerializeField] GameObject gunWeaponControllerPrefab;
        [SerializeField] GameObject heldToolControllerPrefab;


        #region Initializing methods
        
        void Awake()
        {
            player = GetComponent<Player>();
        }

        internal void Initialize()
        {
            armsItemData = Game.Items.GetData("arms");
            InitializeEvents();
            InitializeInputs();
            cameraController.Initialize();

            EquipViewmodelAsync(armsItemData);
            EquipHotbarIndex(1);
            Game.UI.SetCursorVisible(false);
        }

        void OnDestroy()
        {
            DeinitializeInputs();
        }

        void InitializeEvents()
        {
            Player.MoveStateMachine.OnTransition += OnPlayerMoveStateTransition;
            Player.ActionStateMachine.OnTransition += OnPlayerActionStateTransition;
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
        }

        void DeinitializeInputs()
        {
            inputs["Hotbar"].performed -= OnInputHotbar;
            inputs["Reload"].performed -= OnPerformReload;
            inputs["Unholster"].performed -= OnUnholster;
        }

        #endregion

        
        #region Player input callbacks

        void OnInputHotbar(InputAction.CallbackContext context)
        {
            if (!int.TryParse(context.control.displayName, out int index)) return;

            EquipHotbarIndex(index);
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


        #region Public methods

        public void EquipHotbarIndex(int index)
        {
            if (_isWaitingForHotbarSwitch) return;
            if (IsBusy)
            {
                _isWaitingForHotbarSwitch = true;
                OnPerformFinished += OnPerformedFinished;
                void OnPerformedFinished()
                {
                    _isWaitingForHotbarSwitch = false;
                    OnPerformFinished -= OnPerformedFinished;
                    EquipHotbarIndex(index);
                }
            }
            
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
                EquipViewmodelAsync(slot.Item.Data, onEquipCompleted: (viewmodel) =>
                {
                    EquipHotbarItem(slot.Item.Data, index);
                });
            }
            else /// equipped to empty hotbar, so equip arms
            {
                EquipViewmodelAsync(armsItemData, onEquipCompleted: (viewmodel) =>
                {
                    EquipHotbarItem(armsItemData, index);
                });
            }
            
            OnSelectedHotbarIndexChanged?.Invoke(SelectedHotbarIndex);
        }

        /// <summary>
        /// Releases the viewmodel asset via Addressables.
        /// </summary>
        public void ReleaseItem(ItemData data)
        {
            if (false == loadedViewmodelComponents.TryGetValue(data.Id, out var viewmodel)) return;

            Destroy(viewmodel.gameObject); /// salt
            loadedViewmodelComponents.Remove(data.Id);
            
            // var viewmodel = _cachedViewmodels[data.Id];
            UnloadViewmodelAsset(data);
        }

        public void ToggleControls(bool enabled)
        {
            cameraController.ToggleControls(enabled);
        }

        #endregion


        #region Private methods

        void EquipViewmodel(string itemId)
        {
            if (string.IsNullOrEmpty(itemId) || /// invalid id
                _currentEquippedItemId.Equals(itemId) || /// same item to currently equipped
                IsBusy)
            {
                return;
            }

            if (Game.Items.TryGetData(itemId, out var itemData))
            {
                EquipViewmodelAsync(itemData);
            }
        }
        
        /// <summary>
        /// Equip and hold an item to be visible in FPP perspective.
        /// Does nothing if the item does not have a Viewmodel component.
        /// 
        /// If the <seealso cref="ItemData.IsObject"/>, the FPP controller transitions to
        /// building mode.
        /// 
        /// Passing an ItemData as <c>null</c> equips default "arms" item.
        /// </summary>
        async void EquipViewmodelAsync(ItemData itemData, Action<ViewmodelComponent> onEquipCompleted = null)
        {
            if (IsBusy ||
                _currentEquippedItemId.Equals(itemData.Id)) /// same item to currently equipped
            {
                return;
            }

            this.currentHeldItemData = itemData != null ? itemData : armsItemData;
            UnloadCurrentViewmodel();

            if (itemData.IsObject)
            {
                currentViewmodelComponent = loadedViewmodelComponents["arms"];
                PlayAnimations("build_idle");
            }
            else if (itemData is IViewmodelItem viewmodelItem) /// Item has a viewmodel component 
            {
                /// check if we have the viewmodel already loaded
                if (false == loadedViewmodelComponents.ContainsKey(itemData.Id))
                {
                    var viewmodel = await viewmodelController.LoadViewmodelAssetAsync(viewmodelItem);
                    if (viewmodel != null)
                    {
                        /// Cache loaded viewmodel
                        loadedViewmodelComponents[itemData.Id] = viewmodel;
                    }
                }

                currentViewmodelComponent = loadedViewmodelComponents[itemData.Id];
                InitializeCurrentViewmodelComponents();
                PlayAnimations("equip");
            }

            onEquipCompleted?.Invoke(currentViewmodelComponent);
        }

        void EquipHotbarItem(ItemData itemData, int index)
        {
            UnloadCurrentFPPItemController();

            /// check if we have the FPP item already loaded
            if (false == loadedFPPItems.ContainsKey(index))
            {
                var fppItemController = LoadFPPItemController(itemData);
                if (fppItemController != null)
                {
                    loadedFPPItems[index] = fppItemController;
                    InitializeFPPItemController(fppItemController);
                }
            }

            currentFPPItemController = loadedFPPItems[index];
            currentFPPItemController.gameObject.SetActive(true);
            InitializeFPPControllerEvents();

            OnFPPItemChanged?.Invoke(currentFPPItemController);
            OnHeldItemChanged?.Invoke(currentHeldItemData);
            return;
        }

        /// <summary>
        /// Loads the FPP controller for the given Item.
        /// </summary>
        FPPItemController LoadFPPItemController(ItemData itemData)
        {
            FPPItemController fPPItemController = itemData switch
            {
                WeaponData weaponData when weaponData.Category == WeaponCategory.Melee 
                    => LoadFPPItemController<MeleeWeaponController>(meleeWeaponControllerPrefab),

                WeaponData weaponData when weaponData.Category == WeaponCategory.Ranged 
                    => LoadFPPItemController<GunWeaponController>(gunWeaponControllerPrefab),

                ToolData toolData 
                    => LoadFPPItemController<HeldToolController>(heldToolControllerPrefab),

                _ => null
            };

            if (itemData.Subtype == ItemSubtype.Consumable)
            {
                throw new NotImplementedException();
            }
            
            return fPPItemController;
        }

        void InitializeFPPItemController(FPPItemController controller)
        {
            if (controller != null)
            {
                controller.ItemData = currentHeldItemData;
                controller.Viewmodel = currentViewmodelComponent;
                controller.Owner = Player;
                controller.Initialize();

                controller.transform.SetParent(heldItemsContainer.transform);
                controller.name = $"{currentHeldItemData.DisplayName} (Held Item)";
                controller.gameObject.SetActive(true);
            }
        }

        void UnloadViewmodelAsset(ItemData data)
        {
            // await null;
        }

        void UnloadCurrentFPPItemController()
        {
            if (currentFPPItemController != null)
            {
                DeinitializeFPPControllerEvents();
                
                currentFPPItemController.gameObject.SetActive(false);
                currentFPPItemController = null;
            }
        }
        
        /// <summary>
        /// Unholsters the current equipped.
        /// If from arms, switch back to last equipped.
        /// </summary>
        void Unholster()
        {
            if (_isAnimationPlaying) return;

            /// Swaps
            if (_currentEquippedItemId.Equals("arms"))
            {
                EquipViewmodel(_lastEquippedItemId);
            }
            else
            {
                _lastEquippedItemId = currentViewmodelComponent.ItemData.Id;
                EquipViewmodelAsync(armsItemData);
            }
        }

        void PerformReload()
        {
            if (_isAnimationPlaying) return;

            if (currentFPPItemController is IReloadable reloadableWeapon)
            {
                var reloadDuration = GetAnimationClipLength(runtimeViewmodelAnimator, "reload");
                reloadableWeapon.TryReload(reloadDuration);
            }
        }

        void UnloadCurrentViewmodel()
        {
            if (currentViewmodelComponent != null)
            {
                currentViewmodelComponent.gameObject.SetActive(false);
                currentViewmodelComponent = null;
            }
        }

        /// <summary>
        /// Setup the different components of the viewmodel for this FPP Controller.
        /// </summary>
        void InitializeCurrentViewmodelComponents()
    {
            currentViewmodelComponent.gameObject.SetActive(true);

            /// Setup arms animations
            if (currentViewmodelComponent.ViewmodelAnimator != null)
            {
                _hasArmsAnimations = true;
                fppArmsController.SetAnimatorController(currentViewmodelComponent.ViewmodelAnimator.runtimeAnimatorController);
            }
            else
            {
                _hasArmsAnimations = false;
                Game.Console.LogWarn($"Item '{currentViewmodelComponent.ItemData.Id}' has no arms animation.", true);
            }
            
            /// Setup model animations
            if (currentViewmodelComponent.ViewmodelAnimator != null)
            {
                _hasViewmodelAnimations = true;
                runtimeViewmodelAnimator = currentViewmodelComponent.ViewmodelAnimator;
            }
            else
            {
                _hasViewmodelAnimations = false;
                runtimeViewmodelAnimator = null;
                Game.Console.LogWarn($"Item '{currentViewmodelComponent.ItemData.Id}' has no Model Animator. No viewmodel animations would be shown.", true);
            }

            /// Setup camera animations
            if (currentViewmodelComponent.CameraAnimator != null)
            {
                _hasCameraAnimations = true;
                runtimeFPPCameraAnimator = currentViewmodelComponent.CameraAnimator;
            }
            else
            {
                _hasCameraAnimations = false;
                runtimeFPPCameraAnimator = null;
                Game.Console.LogWarn($"Item '{currentViewmodelComponent.ItemData.Id}' has no Camera Animator. No camera animations would be shown.", true);
            }

            if (currentViewmodelComponent.CameraAnimationSource != null)
            {
                cameraAnimationTarget.Source = currentViewmodelComponent.CameraAnimationSource;
            }
            else
            {
                cameraAnimationTarget.Source = null;
            }

            fppArmsController.SetViewmodelSettings(currentViewmodelComponent.ViewmodelData.ViewmodelSettings);
            viewmodelController.SetViewmodelSettings(currentViewmodelComponent.ViewmodelData.ViewmodelSettings);

            return;
        }

        void InitializeFPPControllerEvents()
        {
            switch (currentFPPItemController)
            {
                case MeleeWeaponController meleeWeapon:
                {
                    meleeWeapon.StateMachine.OnTransition += OnMeleeWeaponStateChanged;
                    return;
                }

                case GunWeaponController rangedWeapon:
                {
                    rangedWeapon.StateMachine.OnTransition += OnRangedWeaponStateChanged;
                    rangedWeapon.OnFire += OnRangedWeaponFire;
                    return;
                }

                case HeldToolController tool:
                {
                    tool.StateMachine.OnTransition += OnHeldToolStateChanged;
                    return;
                }
            }
        }

        void DeinitializeFPPControllerEvents()
        {
            switch (currentFPPItemController)
            {
                case MeleeWeaponController meleeWeapon:
                {
                    meleeWeapon.StateMachine.OnTransition -= OnMeleeWeaponStateChanged;
                    return;
                }

                case GunWeaponController rangedWeapon:
                {
                    rangedWeapon.StateMachine.OnTransition -= OnRangedWeaponStateChanged;
                    rangedWeapon.OnFire -= OnRangedWeaponFire;
                    return;
                }

                case HeldToolController tool:
                {
                    tool.StateMachine.OnTransition -= OnHeldToolStateChanged;
                    return;
                }
            }
        }


        #region Event callbacks

        void OnPlayerMoveStateTransition(StateMachine<MoveStates>.TransitionContext transition)
        {

        }
        
        void OnPlayerActionStateTransition(StateMachine<ActionStates>.TransitionContext transition)
        {
            if (IsBusy) return;

            if (transition.To == ActionStates.Secondary)
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
                    EquipViewmodel("arms");
                }
            }
            else /// added Item to hotbar
            {
                if (IsCurrentlySelectedHotbar(e.ItemSlot.Index + 3)) /// + 3 cause the QUOTES HOTBAR QUOTES STARTS AT 3
                {
                    /// player has this slot equipped, immediately hold the new Item ig
                    EquipViewmodelAsync(e.NewItem.Data);
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
            if (context.To == MeleeWeaponStates.Attack)
            {
                var animId = GetAnimIdFromState(context.To);
                if (currentFPPItemController is MeleeWeaponController meleeWeapon)
                {
                    PlayAnimations($"{animId}_{meleeWeapon.ComboCounter + 1}");
                }
            }
        }

        void OnRangedWeaponStateChanged(StateMachine<GunWeaponStates>.TransitionContext e)
        {
            var animId = GetAnimIdFromState(e.To);
            PlayAnimations(animId);
        }

        void OnRangedWeaponFire()
        {
            HandleGunWeaponRecoil();
        }

        void OnHeldToolStateChanged(StateMachine<ToolItemStates>.TransitionContext context)
        {
            if (currentFPPItemController == null) return; /// TODO: optimiuze by subbing a flag instead

            if (context.To == ToolItemStates.Attack)
            {
                var animId = GetAnimIdFromState(context.To);
                if (currentFPPItemController is HeldToolController tool)
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
                fppArmsController.PlayAnimation(animId);
            }
            if (_hasViewmodelAnimations)
            {
                runtimeViewmodelAnimator.Play(animId, 0, 0f);
            }
            if (_hasCameraAnimations)
            {
                runtimeFPPCameraAnimator?.Play(animId, 0, 0f);
                cameraAnimationTarget.PlayAnimation();
            }

            /// viewmodelAnimator is used here because it's the one that
            /// usually has animations first :P idk tho
            var animLengthSeconds = GetAnimationClipLength(runtimeViewmodelAnimator, animId);
            Timing.KillCoroutines(_fppAnimationCoroutineHandle);
            _fppAnimationCoroutineHandle = Timing.RunCoroutine(_PerformAnimationCoroutine(animLengthSeconds));
        }
        
        #endregion


        void HandleGunWeaponRecoil()
        {
            if (currentFPPItemController is GunWeaponController weapon)
            {
                var weaponData = weapon.WeaponData;
                var recoilInfo = weaponData.RangedAttributes.RecoilAttributes;

                cameraController.AddRecoilMotion(recoilInfo);
            }
        }
        
        T LoadFPPItemController<T>(GameObject prefab) where T : FPPItemController
        {
            var go = Instantiate(prefab);
            if (false == go.TryGetComponent(out T controller))
            {
                Game.Console.LogWarn($"Loaded prefab does not contain a component of type {typeof(T)}.", true);
                Destroy(go);
            }

            return controller;
        }

        IEnumerator<float> _PerformAnimationCoroutine(float durationSeconds)
        {
            _isAnimationPlaying = true;

            yield return Timing.WaitForSeconds(durationSeconds);

            _isAnimationPlaying = false;
            cameraAnimationTarget.StopAnimation();
            OnPerformFinished?.Invoke();
            
            yield break;
        }

        #endregion
        

        #region Helper functions

        static float GetAnimationClipLength(Animator animator, string name)
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
