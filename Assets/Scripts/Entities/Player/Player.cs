using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;

using MEC;
using TMPro;

using UZSG.Attributes;
using UZSG.Building;
using UZSG.Data;
using UZSG.EOS;
using UZSG.FPP;
using UZSG.Interactions;
using UZSG.Inventory;
using UZSG.Network;
using UZSG.Players;
using UZSG.Saves;
using UZSG.StatusEffects;
using UZSG.UI;
using UZSG.UI.HUD;
using UZSG.UI.Players;
using UZSG.Worlds;
using static UZSG.Players.MoveStates;

namespace UZSG.Entities
{
    /// <summary>
    /// Player entity.
    /// </summary>
    public partial class Player : Entity, IPlayer
    {
        public string DisplayName
        {
            get => this.NetworkEntity.AccountInfo.DisplayName ?? "Player";
            set => nametagText.text = value;
        }
        public bool CanPickUpItems = true;

        public PlayerEntityData PlayerEntityData => (PlayerEntityData) entityData;
        public PlayerSaveData SaveData => (PlayerSaveData) saveData;
        
        [SerializeField] PlayerInventoryManager inventory;
        public PlayerInventoryManager Inventory => inventory;

        // [SerializeField] PlayerCrafting crafter;
        // public PlayerCrafting Crafter => crafter;
        [SerializeField] PlayerCrafting crafting;
        public PlayerCrafting Crafting => crafting;

        [SerializeField] PlayerBuildingManager buildingManager;
        public PlayerBuildingManager BuildManager => buildingManager;

        [SerializeField] StatusEffectCollection statusEffects;
        public StatusEffectCollection StatusEffects => statusEffects;
        
        [SerializeField] PlayerAudioSourceController audioController;
        public PlayerAudioSourceController Audio => audioController;

        [SerializeField] EntityHitboxController hitboxes;
        public EntityHitboxController Hitboxes => hitboxes;

        PlayerAbsoluteTerritory at;
        /// <summary>
        /// The world this player is currently in.
        /// </summary>
        public World World { get; private set; }
        bool _isInitialized = false;
        bool _inGodMode = false;
        public bool InGodMode => _inGodMode;
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
        public override Vector3 Position
        {
            get => Rigidbody.position;
            set => Rigidbody.position = value;
        }
        public override Quaternion Rotation
        {
            get => Model.rotation;
            set => Model.rotation = value;
        }
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
                if (Attributes != null &&
                    Attributes.TryGet(AttributeId.Stamina, out var stamina) &&
                    Attributes.TryGet(AttributeId.JumpStaminaCost, out var jumpStaminaCost))
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
        [SerializeField] Camera mainCamera;
        /// <summary>
        /// Unity camera tagged "MainCamera"
        /// </summary>
        public Camera MainCamera => mainCamera;
        public PlayerControls Controls { get; private set; }
        public PlayerActions Actions { get; private set; }
        public MovementStateMachine MoveStateMachine { get; private set; }
        public ActionStateMachine ActionStateMachine { get; private set; }
        public FPPController FPP { get; private set; }
        public Rigidbody Rigidbody => Controls.Rigidbody;

        [Space, Header("Network Components")]
        [SerializeField] NetworkObject networkObject;
        public NetworkObject NetworkObject => networkObject;
        [SerializeField] PlayerNetworkEntity networkEntity;
        public PlayerNetworkEntity NetworkEntity => networkEntity;

        [SerializeField] GameObject nametagGameObject;
        [SerializeField] TextMeshPro nametagText;
        [SerializeField] List<GameObject> clientObjects = new();


        #region Initializing methods

        void Awake()
        {
            inventory = GetComponentInChildren<PlayerInventoryManager>();
            crafting = GetComponentInChildren<PlayerCrafting>();
            buildingManager = GetComponentInChildren<PlayerBuildingManager>();
            at = GetComponentInChildren<PlayerAbsoluteTerritory>();
            MoveStateMachine = GetComponent<MovementStateMachine>();
            ActionStateMachine = GetComponent<ActionStateMachine>();
            Controls = GetComponent<PlayerControls>();
            Actions = GetComponent<PlayerActions>();
            FPP = GetComponent<FPPController>();
        }

        public override void OnSpawnEvent()
        {
            Initialize();
            // Game.Console.LogInfo("I, player, has been spawned!");
        }

        /// <summary>
        /// Initialize the player as a normal entity.
        /// </summary>
        public void Initialize()
        {
            /// Initialize all Player Entity components (order important)
            audioController.CreateAudioPool(8);
            audioController.LoadAudioAssetsData(PlayerEntityData.AudioAssetsData); /// TODO: should be global player sounds only

            InitializeStateMachines();
            InitializeAnimator();
            InitializeHitboxes();
            World = Game.World.CurrentWorld;
        }

        /// <summary>
        /// Initializing a player entity as local player means to enable all client things associated with it
        /// (e.g., inputs, actions, camera renders, etc)
        /// </summary>
        public void InitializeAsPlayer(PlayerSaveData saveData, bool isLocalPlayer)
        {
            if (_isInitialized) return;
            
            ReadSaveData(saveData as PlayerSaveData); 
    
            InitializeInventoryUIs();
            InitializeHUDs();

            crafting.Initialize(this, isLocalPlayer);  /// TODO: SAVEABLE
            buildingManager.Initialize(this, isLocalPlayer);

            if (isLocalPlayer)
            {
                ActivateClientComponents();
                InitializeAttributeEvents();
                InitializeAnimatorAsClient();
                InitializeClientEvents();
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
            }

            Game.Tick.OnTick += OnTick;
            _isInitialized = true;
            OnDoneInit?.Invoke(this);
        }

        void InitializeClientEvents()
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
        }

        void InitializeStateMachines()
        {
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

        /// <summary>
        /// Client components are initially disabled.
        /// </summary>
        void ActivateClientComponents()
        {
            foreach (GameObject go in clientObjects)
            {
                go.SetActive(true);
            }
            MainCamera.gameObject.SetActive(true);
        }

        #endregion


        #region Event callbacks

        void Update()
        {
            UpdateAnimator();

        }

        public void OnTriggerEnter(Collider other)
        {
            switch (other.gameObject.tag)
            {
                case Tags.WATER:
                {
                    print("enter swim mode");
                    ToggleSwim(true);
                    break;
                }
            }
        }

        public void OnTriggerExit(Collider other)
        {
            switch (other.gameObject.tag)
            {
                case Tags.WATER:
                {
                    print("exit swim mode");
                    ToggleSwim(false);
                    break;
                }
            }
        }

        void ToggleSwim(bool enable)
        {

        }

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
            Despawn(notify: false);
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
            if (Attributes.TryGet(AttributeId.Stamina, out var stamina))
            {
                float jumpStaminaCost = Attributes.Get("jump_stamina_cost").Value;
                stamina.Remove(jumpStaminaCost);
            }
            /// Consume Hunger on jump
            if (Attributes.TryGet(AttributeId.Hunger, out var hunger))
            {
                float jumpHungerCost = Attributes.Get("jump_hunger_cost").Value;
                hunger.Remove(jumpHungerCost);
            }
            /// Consume Hydration on jump
            if (Attributes.TryGet(AttributeId.Hydration, out var hydration))
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
            if (saveData == null || PlayerSaveData.IsEmpty(saveData))
            {
                Game.Console.LogDebug($"Tried to load a save data for player but it was invalid. Loading defaults instead...");
                LoadDefaultSaveData<PlayerSaveData>();
                saveData = this.saveData as PlayerSaveData; /// yes, override the parameter
            }
            
            this.saveData = saveData;

            ReadTransformSaveData(saveData.Transform);
            
            attributes = new();
            attributes.ReadSaveData(saveData.Attributes);

            inventory.Initialize(this);
            inventory.ReadSaveData(saveData.Inventory);
        }

        /// stuff to save?
        /// - currentlsy selected hotbar
        /// - known recipes
        /// - player crafter (workstation) data
        public new PlayerSaveData WriteSaveData()
        {
            var esd = base.WriteSaveData();
            
            string uid = World.LOCAL_PLAYER_ID;
            if (EOSSubManagers.Auth.IsLoggedIn)
            {
                uid = Game.EOS.GetLocalUserId().ToString();
            }
            
            var psd = new PlayerSaveData
            {
                UID = uid,
                Id = esd.Id,
                Transform = esd.Transform,
                Attributes = attributes.WriteSaveData(),
                Inventory = inventory.WriteSaveData()
            };
            psd.Transform.Rotation = Utils.ToFloatArray(FPP.Camera.LocalRotationEuler);

            return psd;
        }

        protected override void ReadTransformSaveData(TransformSaveData data)
        {
            data ??= new();
            Rigidbody.position = Utils.FromFloatArray(data.Position);
            FPP.Camera.LookRotation(Utils.FromFloatArray(data.Rotation));
            // transform.localScale = Utils.FromFloatArray(data.LocalScale);
        }

        #endregion


        public void TakeDamage(DamageInfo damageInfo)
        {
            if (attributes.TryGet(AttributeId.Health, out var health))
            {
                health.Value -= damageInfo.Amount;
            }
        }

        public void UseObjectGUI(ObjectGUI gui)
        {
            gui.SetPlayer(this);
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

        public void SetNametagVisible(bool visible)
        {
            nametagGameObject.SetActive(visible);
        }

        RaycastHit[] raycastHits = new RaycastHit[8];
        /// <summary>
        /// Check if the player can see the given entity from their FPP camera.
        /// /// </summary>
        public override bool CanSee(Entity entity)
        {
            /// TODO: does not work lol 
            Vector3 direction = (this.Position - this.EyeLevel).normalized;
            float distance = Vector3.Distance(this.Position, entity.Position);

            var origin = this.EyeLevel + Forward.normalized * 0.25f; /// a little bit forward so it does not hit the Player itself; it still hits it, not too far or bug
            int count = Physics.RaycastNonAlloc(origin, direction, raycastHits, distance);
            for (int i = 1; i < count; i++) /// starting 1 skips the player T-T
            {
                var hit = raycastHits[i];
                if (hit.collider.TryGetComponent<Entity>(out var detected) &&
                    detected == entity) /// check if same entity
                {
                    return true;
                }
            }

            return false;
        }

        public void AddExperience(int amount, bool shareWithPartyMembers = false)
        {
            if (attributes.TryGet("current_experience", out var exp))
            {
                exp.Add(amount);
            }

            if (shareWithPartyMembers)
            {
                throw new NotImplementedException();
            }
        }

        public void SetGodMode(bool enabled)
        {
            _inGodMode = enabled;
        }
    }
}