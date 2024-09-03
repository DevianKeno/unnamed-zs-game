using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.InputSystem;
using MEC;

using UZSG.Systems;
using UZSG.Inventory;
using UZSG.Interactions;
using UZSG.Attributes;
using UZSG.Players;
using UZSG.FPP;
using UZSG.UI.HUD;
using UZSG.UI.Players;
using UZSG.Crafting;
using UZSG.StatusEffects;
using UZSG.Saves;
using UZSG.UI.Objects;
using UZSG.UI;

using static UZSG.Players.MoveStates;   

namespace UZSG.Entities
{
    /// <summary>
    /// Player entity.
    /// </summary>
    public partial class Player : Entity, IPlayer
    {
        public bool CanPickUpItems = true;

        public PlayerEntityData PlayerEntityData => (PlayerEntityData) entityData;
        public PlayerSaveData SaveData => (PlayerSaveData) saveData;
        
        [SerializeField] InventoryHandler inventory;
        public InventoryHandler Inventory => inventory;

        [SerializeField] PlayerCrafting crafter;
        public PlayerCrafting Crafter => crafter;

        [SerializeField] StatusEffectCollection statusEffects;
        public StatusEffectCollection StatusEffects => statusEffects;

        [SerializeField] PlayerAudioSourceController audioController;
        public PlayerAudioSourceController Audio => audioController;

        [SerializeField] EntityHitboxController hitboxes;
        public EntityHitboxController Hitboxes => hitboxes;
        
        InventoryUI invUI;
        public InventoryUI InventoryGUI => invUI;
        PlayerHUDVitals _vitalsHUD;
        public PlayerHUDVitals VitalsHUD => _vitalsHUD;
        PlayerHUDInfo _infoHUD;
        public PlayerHUDInfo InfoHUD => _infoHUD;        
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
        
        /// <summary>
        /// A full jump requires complete stamina cost.
        /// </summary>
        public bool HasStaminaForJump
        {
            get
            {
                return Attributes["stamina"].Value >= Attributes["jump_stamina_cost"].Value;
            }
        }
    
        #endregion


        #region Events

        public event EventHandler<EventArgs> OnDoneInit;

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
            LoadDefaultSaveData<PlayerSaveData>();
            ReadSaveData((PlayerSaveData) saveData);

            Initialize();
        }

        void Initialize()
        {
            Game.Console.Log("I, player, has been spawned!");

            audioController.CreateAudioPool(8);
            audioController.LoadAudioAssetsData(PlayerEntityData.AudioAssetsData);

            InitializeAttributes();
            InitializeStateMachines();
            InitializeHitboxes();
            InitializeAnimator();
            InitializeInventory();
            // InitializeCrafter();
            InitializeHUD();
            InitializeInputs();
            
            Controls.Initialize();
            Controls.Enable();
            Actions.Initialize();
            Actions.Enable();
            FPP.Initialize();
            ParentMainCameraToFPPController();

            Game.Tick.OnTick += Tick;
            OnDoneInit?.Invoke(this, new());
        }
        
        void InitializeAttributes()
        {
            attributes["stamina"].OnValueModified += OnAttrStaminaModified;
            currentHealth = Attributes.Get("health").Value;
        }

        void InitializeStateMachines()
        {
            MoveStateMachine.InitialState = MoveStateMachine.States[Idle];

            MoveStateMachine.OnTransition += TransitionAnimator;
            MoveStateMachine.OnTransition += MoveTransitionCallback;
            /// Moved to PlayerAnimator.cs
            // MoveStateMachine.States[MoveStates.Crouch].OnTransition += OnCrouchState;
        }

        void InitializeHitboxes()
        {
            foreach (var hitbox in hitboxes.Hitboxes)
            {
                Debug.Log("Is hitting body part: " + hitbox.name);
                hitbox.OnCollision += OnHitboxCollision;
            }
        }

        void InitializeHUD()
        {
            _infoHUD = Game.UI.Create<PlayerHUDInfo>("Player HUD Info");
            _infoHUD.Initialize(this);

            _vitalsHUD = Game.UI.Create<PlayerHUDVitals>("Player HUD Vitals");
            _vitalsHUD.Initialize(this);
        }

        void InitializeInventory()
        {
            inventory.Initialize();
            // inventory.ReadSaveJson(new());
            // inventory.ReadSaveJson(saveData.Inventory);

            invUI = Game.UI.Create<InventoryUI>("Player Inventory", show: false);
            invUI.Initialize(this);

            invUI.OnOpen += () =>
            {
                Actions.Disable();
                inputs["Look"].Disable();
                _infoHUD.Hide();
            };
            invUI.OnClose += () =>
            {
                Actions.Enable();
                inputs["Look"].Enable();
                _infoHUD.Show();
            };
        }

        void InitializeCrafter()
        {
            crafter.InitializePlayer(this);
            // craftingAgent.AddContainer(inventory.Bag);
            // craftingAgent.AddContainer(inventory.Hotbar);
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


        void OnDestroy()
        {
            Game.Tick.OnTick -= Tick;
        }
        
        void Tick(TickInfo t)
        {
            InnateConsumption();
            ConsumeStaminaWhileRunning();
            RegenerateStamina();
        }


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
                    JumpAction();
                    break;
                }
                default:
                {
                    break;
                }
            }
        }

        void JumpAction()
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
            invUI.ToggleVisibility();
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

        public void ReadSaveData(PlayerSaveData saveData)
        {
            if (saveData == null)
            {
                Game.Console.LogError($"Invalid PlayerSaveData loaded for Player.");
                return;
            }
            
            base.ReadSaveData(saveData);

            /// Load inventory, etc. and whatever the fuck not related to the Player
            // Inventory.ReadSaveData(saveData.Inventory);
        }
        
        public new PlayerSaveData WriteSaveData()
        {
            var esd = base.WriteSaveData();
            var psd = new PlayerSaveData
            {
                Id = esd.Id,
                Transform = esd.Transform,
                Attributes = attributes.WriteSaveData(),
                Inventory = inventory.WriteSaveData()
            };

            return psd;
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