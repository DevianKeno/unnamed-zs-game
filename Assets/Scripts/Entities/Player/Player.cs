using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.InputSystem;
using MEC;

using Epic.OnlineServices.UserInfo;

using UZSG.Systems;
using UZSG.Inventory;
using UZSG.Interactions;
using UZSG.Attributes;
using UZSG.Players;
using UZSG.FPP;
using UZSG.UI.HUD;
using UZSG.UI.Players;
using UZSG.StatusEffects;
using UZSG.Saves;
using UZSG.UI.Objects;
using UZSG.UI;
using UZSG.Building;
using UZSG.Data;

using static UZSG.Players.MoveStates;
using UZSG.EOS;

namespace UZSG.Entities
{
    /// <summary>
    /// Player entity. (client)
    /// </summary>
    public partial class Player : Entity, IPlayer
    {
        public UserInfoData UserInfo { get; set; }
        public string DisplayName => this.UserInfo.DisplayName ?? "Player";
        public bool CanPickUpItems = true;

        public PlayerEntityData PlayerEntityData => (PlayerEntityData) entityData;
        public PlayerSaveData SaveData => (PlayerSaveData) saveData;
        
        [SerializeField] InventoryHandler inventory;
        public InventoryHandler Inventory => inventory;

        // [SerializeField] PlayerCrafting crafter;
        // public PlayerCrafting Crafter => crafter;
        [SerializeField] Players.PlayerCrafting crafting;
        public Players.PlayerCrafting Crafting => crafting;

        [SerializeField] BuildingManager buildingManager;
        public BuildingManager BuildManager => buildingManager;

        [SerializeField] StatusEffectCollection statusEffects;
        public StatusEffectCollection StatusEffects => statusEffects;

        [SerializeField] PlayerAudioSourceController audioController;
        public PlayerAudioSourceController Audio => audioController;

        [SerializeField] EntityHitboxController hitboxes;
        public EntityHitboxController Hitboxes => hitboxes;
        
        bool _isInitialized = false;
        /// <summary>
        /// List of UI elements that are attached to the Player.
        /// </summary>
        List<UIElement> uiElements = new();
        InventoryWindow invUI;
        public InventoryWindow InventoryWindow => invUI;
        PlayerHUDVitalsUI vitalsHUD;
        public PlayerHUDVitalsUI VitalsHUD => vitalsHUD;
        PlayerHUDInfoUI infoHUD;
        public PlayerHUDInfoUI InfoHUD => infoHUD;        
        InputActionMap actionMap;
        readonly Dictionary<string, InputAction> inputs = new();


        #region Properties

        /// <summary>
        /// Player's forward direction relative to the FPP Camera.
        /// </summary>
        public Vector3 Forward => MainCamera.transform.forward;
        /// <summary>
        /// Player's right direction relative to the FPP Camera.
        /// </summary>
        public Vector3 Right => MainCamera.transform.right;
        /// <summary>
        /// Player's upward direction relative to the FPP Camera.
        /// </summary>
        public Vector3 Up => MainCamera.transform.up;
        /// <summary>
        /// World space position of the Player's eye level.
        /// </summary>
        public Vector3 EyeLevel => MainCamera.transform.position;
        public Transform Model => Controls.Model;
        public bool HasHeldItem { get; protected set; } = false;
        /// <summary>
        /// Mutable! Make a copy if you plan on referencing.
        /// </summary>
        public ItemData HeldItem { get; protected set; } = null;
        /// <summary>
        /// A full jump requires complete stamina cost.
        /// </summary>
        public bool HasStaminaForFullJump
        {
            get
            {
                if (Attributes != null && Attributes.TryGet("stamina", out var stamina))
                if (Attributes.TryGet("jump_stamina_cost", out var jumpStaminaCost))
                {
                    return stamina.Value >= jumpStaminaCost.Value;
                }
                return false;
            }
        }
    
        #endregion


        #region Events

        public event Action<Player> OnDoneInit;

        #endregion


        [field: Header("Components")]
        /// <summary>
        /// Unity camera tagged "MainCamera"
        /// </summary>
        public Camera MainCamera { get; private set; }
        public PlayerControls Controls { get; private set; }
        public PlayerActions Actions { get; private set; }
        public MovementStateMachine MoveStateMachine { get; private set; }
        public ActionStateMachine ActionStateMachine { get; private set; }
        public FPPController FPP { get; private set; }
        public Rigidbody Rigidbody => Controls.Rigidbody;


        #region Initializing methods

        void Awake()
        {
            MainCamera = Camera.main;
            MoveStateMachine = GetComponent<MovementStateMachine>();
            ActionStateMachine = GetComponent<ActionStateMachine>();
            Controls = GetComponent<PlayerControls>();
            Actions = GetComponent<PlayerActions>();
            FPP = GetComponent<FPPController>();
        }

        public override void OnSpawn()
        {
            // Game.Console.LogInfo("I, player, has been spawned!");
            audioController.CreateAudioPool(8);
            audioController.LoadAudioAssetsData(PlayerEntityData.AudioAssetsData); /// TODO: should be global player sounds only
        }

        public void Initialize(PlayerSaveData saveData = null)
        {
            if (_isInitialized) return;
            
            /// Data handle
            if (saveData == null)
            {
                LoadDefaultSaveData<PlayerSaveData>();
            }
            else
            {
                this.saveData = saveData;
            }
            this.ReadSaveData(this.saveData as PlayerSaveData);
            
            /// Components handle (order important)
            InitializeAttributeEvents();
            InitializeStateMachines();
            InitializeHitboxes();
            InitializeAnimator();
            InitializeInventoryUIs();
            InitializeCrafter();     /// TODO: SAVEABLE
            InitializeBuilding();
            InitializeHUDs();
            InitializeEvents();
            InitializeInputs();

            Controls.Initialize();
            Controls.Enable();
            Actions.Initialize();
            Actions.Enable();
            FPP.Initialize();
            FPP.OnHeldItemChanged += (heldItemData) =>
            {
                HasHeldItem = heldItemData != null;
                HeldItem = heldItemData;
            };
            ParentMainCameraToFPPController();

            this.Rigidbody.isKinematic = false;
            Game.Tick.OnTick += OnTick;
            _isInitialized = true;
            OnDoneInit?.Invoke(this);
        }

        void InitializeInventory()
        {
            inventory.Initialize(this);
        }

        void InitializeEvents()
        {
            Game.World.OnExitWorld += OnExitWorld;
            Game.World.CurrentWorld.OnPause += OnPause;
            Game.World.CurrentWorld.OnUnpause += OnUnpause;

            Game.Console.Gui.OnOpened += OnConsoleGuiOpened;
            Game.Console.Gui.OnClosed += OnConsoleGuiClosed;

            Game.UI.OnAnyWindowOpened += OnAnyWindowOpened;
            Game.UI.OnAnyWindowClosed += OnAnyWindowClosed;
        }

        void InitializeAttributeEvents()
        {
            attributes["stamina"].OnValueModified += OnAttrStaminaModified;
            currentHealth = Attributes.Get("health").Value;
        }

        void InitializeStateMachines()
        {
            MoveStateMachine.InitialState = MoveStateMachine.States[Idle];

            MoveStateMachine.OnTransition += TransitionAnimator;
            MoveStateMachine.OnTransition += MoveTransitionCallback;
        }

        void InitializeHitboxes()
        {
            hitboxes.ReinitializeHitboxes();
            foreach (var hitbox in hitboxes.Hitboxes)
            {
                // Debug.Log("Is hitting body part: " + hitbox.name);
                hitbox.OnCollision += OnHitboxCollision;
            }
        }

        void InitializeHUDs()
        {
            infoHUD = Game.UI.Create<PlayerHUDInfoUI>("Player HUD Info");
            infoHUD.Initialize(this);
            uiElements.Add(infoHUD);

            vitalsHUD = Game.UI.Create<PlayerHUDVitalsUI>("Player HUD Vitals");
            vitalsHUD.Initialize(this);
            uiElements.Add(vitalsHUD);
        }

        void InitializeInventoryUIs()
        {
            invUI = Game.UI.Create<InventoryWindow>("Player Inventory");
            invUI.Initialize(this);
            invUI.Hide();
            uiElements.Add(invUI);

            invUI.OnOpened += () =>
            {
                Actions.Disable();
                inputs["Look"].Disable();
                infoHUD.Hide();
            };
            invUI.OnClosed += () =>
            {
                Actions.Enable();
                inputs["Look"].Enable();
                infoHUD.Show();
            };
        }

        void InitializeCrafter()
        {
            crafting.Initialize(this);
        }

        void InitializeBuilding()
        {
            buildingManager.Initialize(this);
        }

        void InitializeInputs()
        {
            actionMap = Game.Main.GetActionMap("Player");
            foreach (var action in actionMap.actions)
            {
                inputs[action.name] = action;
                action.Enable();
            }
            
            inputs["Inventory"].performed += OnPerformInventory;        // Tab/E (default)
        }

        void ParentMainCameraToFPPController()
        {
            MainCamera.transform.SetParent(FPP.Camera.Holder, worldPositionStays: false);
            MainCamera.transform.localPosition = Vector3.zero;
        }

        #endregion


        #region Event callbacks

        void OnConsoleGuiOpened()
        {
            inputs["Look"].Disable();
            Controls.Disable();
            Actions.Disable();
        }

        void OnConsoleGuiClosed()
        {
            inputs["Look"].Enable();
            Controls.Enable();
            Actions.Enable();
        }

        void OnAnyWindowOpened(Window window)
        {
            Actions.Disable();
        }

        void OnAnyWindowClosed(Window window)
        {
            Actions.Enable();
        }

        void OnExitWorld()
        {
            /// TODO: player itself shoule be first on notifying itself that it had exited the world
            Game.World.OnExitWorld -= OnExitWorld;

            Game.Console.Gui.OnOpened -= OnConsoleGuiOpened;
            Game.Console.Gui.OnClosed -= OnConsoleGuiClosed;
            inputs["Inventory"].performed -= OnPerformInventory;
            DestroyAllUIElements();
            Kill(notify: false);
        }

        void OnPause()
        {
            Game.Main.GetActionMap("Player").Disable();
        }

        void OnUnpause()
        {
            Game.Main.GetActionMap("Player").Enable();
        }

        void OnDestroy()
        {
            Game.Tick.OnTick -= OnTick;
        }
        
        void OnTick(TickInfo t)
        {
            InnateConsumption();
            ConsumeStaminaWhileRunning();
            RegenerateStamina();
        }

        #endregion


        #region Attribute events callbacks

        bool _allowStaminaRegen;
        CoroutineHandle _delayedStaminaTimer;
        void OnAttrStaminaModified(object sender, AttributeValueChangedContext e)
        {
            if (e.ValueChangedType == UZSG.Attributes.Attribute.ValueChangeType.Decreased)
            {
                _allowStaminaRegen = false;
                Timing.KillCoroutines(_delayedStaminaTimer);
                _delayedStaminaTimer = Timing.RunCoroutine(_DelayStaminaRegen());
            }
        }

        #endregion


        #region Move state machine callbacks

        void MoveTransitionCallback(StateMachine<MoveStates>.TransitionContext transition)
        {
            switch (transition.To)
            {
                case Jump:
                {
                    ConsumeStatsBecauseJump();
                    break;
                }
                default:
                {
                    break;
                }
            }
        }

        void ConsumeStatsBecauseJump() /// HAHAHA
        {
            /// Consume Stamina on jump
            if (Attributes.TryGet("stamina", out var stamina))
            {
                float jumpStaminaCost = Attributes.Get("jump_stamina_cost").Value;
                stamina.Remove(jumpStaminaCost);
            }
            /// Consume Hunger on jump
            if (Attributes.TryGet("hunger", out var hunger))
            {
                float jumpHungerCost = Attributes.Get("jump_hunger_cost").Value;
                hunger.Remove(jumpHungerCost);
            }
            /// Consume Hydration on jump
            if (Attributes.TryGet("hydration", out var hydration))
            {
                float jumpHydrationCost = Attributes.Get("jump_hydration_cost").Value;
                hydration.Remove(jumpHydrationCost);
            }
        }

        #endregion


        #region Player input callbacks
        
        void OnPerformInventory(InputAction.CallbackContext context)
        {
            if (invUI.IsVisible)
                invUI.Hide();
            else
                invUI.Show();
        }

        #endregion


        void OnHitboxCollision(object sender, HitboxCollisionInfo info)
        {
            if (info.Source is Bullet bullet)
            {
                // var hitbox = sender as Hitbox;

                // TakeDamage(10f);

                // SpawnBlood(info.ContactPoint);
                // // SpawnDamageText(info.ContactPoint);
                // Destroy(bullet.gameObject); /// Change on penetration
            }
            // else if (info.Source is MeleeWeaponController meleeWeapon)
            {
                // TakeDamage(10f);

                // SpawnBlood(info.ContactPoint);
                // // SpawnDamageText(info.ContactPoint);
            }
        }
        

        #region Public methods

        #region Saving/loading

        /// <summary>
        /// Reads and initializes all save data. Includes
        ///     - Attributes
        ///     - Inventory
        /// </summary>
        public void ReadSaveData(PlayerSaveData saveData)
        {
            if (saveData == null)
            {
                Game.Console.LogError($"Invalid PlayerSaveData loaded for Player.");
                return;
            }
            
            base.ReadSaveData(saveData); /// as Entity

            /// Load inventory, etc. and whatever the fuck not related to the Player
            this.inventory.Initialize(this);
            this.inventory.ReadSaveData(saveData.Inventory);
        }

        /// stuff to save?
        /// - currentlsy selected hotbar
        /// - known recipes
        /// - player crafter (workstation) data
        public new PlayerSaveData WriteSaveData()
        {
            var esd = base.WriteSaveData();
        
            var userId = this.UserInfo.UserId;
            var psd = new PlayerSaveData
            {
                UID = userId != null ? userId.ToString() :  "localplayer",
                Id = esd.Id,
                Transform = esd.Transform,
                Attributes = attributes.WriteSaveData(),
                Inventory = inventory.WriteSaveData()
            };
            esd.Transform.Rotation = Utils.FromUnityVec3(FPP.Camera.LocalRotationEuler);

            return psd;
        }

        protected override void ReadTransformSaveData(TransformSaveData data)
        {
            this.Rigidbody.position = Utils.FromNumericVec3(data.Position);
            this.FPP.Camera.LookRotation(Utils.FromNumericVec3(data.Rotation));
            transform.localScale = Utils.FromNumericVec3(data.LocalScale);
        }

        #endregion


        public void UseObjectGUI(ObjectGUI gui)
        {
            invUI.AppendObjectGUI(gui, 1);
        }

        public void RemoveObjectGUI(ObjectGUI gui)
        {
            invUI.RemoveObjectGUI(gui);
        }

        void DestroyAllUIElements()
        {
            foreach (UIElement element in uiElements)
            {
                element.Destruct();
            }
        }
        
        #endregion


        void InnateConsumption()
        {
            /// Innate hunger consumption
            
        }

        void RegenerateStamina()
        {
            if (_allowStaminaRegen)
            {
                var regenValue = Attributes.Get("stamina_regen_per_tick").Value;
                Attributes.Get("stamina").Add(regenValue);
            }
        }

        void ConsumeStaminaWhileRunning()
        {
            if (Controls.IsRunning && Controls.IsMoving)
            {
                var runStaminaCost = Attributes.Get("run_stamina_cost").Value;
                Attributes.Get("stamina").Remove(runStaminaCost);
            }
        }
        
        IEnumerator<float> _DelayStaminaRegen()
        {
            yield return Timing.WaitForSeconds(Attributes["stamina_regen_delay"].Value);
            _allowStaminaRegen = true;
        }
    }
}