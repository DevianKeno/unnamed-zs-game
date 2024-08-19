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

namespace UZSG.Entities
{
    public interface IPlayer : IAttributable, IInteractActor, ISaveDataReadWrite<PlayerSaveData>
    {
    }
    
    /// <summary>
    /// Player entity.
    /// </summary>
    public partial class Player : Entity, IPlayer
    {
        public bool CanPickUpItems = true;

        PlayerSaveData saveData;
        public PlayerSaveData SaveData => saveData;
        
        [SerializeField] PlayerEntityData playerEntityData;
        public PlayerEntityData PlayerEntityData => playerEntityData;

        [SerializeField] AttributeCollection attributes;
        public AttributeCollection Attributes => attributes;

        [SerializeField] InventoryHandler inventory;
        public InventoryHandler Inventory => inventory;

        [SerializeField] PlayerCrafting crafter;
        public PlayerCrafting Crafter => crafter;

        [SerializeField] StatusEffectCollection statusEffects;
        public StatusEffectCollection StatusEffects => statusEffects;

        [SerializeField] PlayerAudioSourceController audioController;
        public PlayerAudioSourceController Audio => audioController;

        public Vector3 Forward => MainCamera.transform.forward;
        public Vector3 Right => MainCamera.transform.right;
        public Vector3 Up => MainCamera.transform.up;
        public Vector3 EyeLevel => MainCamera.transform.position;
        public Transform Model => Controls.Model;

        InventoryUI invUI;
        public InventoryUI InventoryGUI => invUI;
        PlayerHUDVitals _vitalsHUD;
        public PlayerHUDVitals VitalsHUD => _vitalsHUD;
        PlayerHUDInfo _infoHUD;
        public PlayerHUDInfo InfoHUD => _infoHUD;

        
        InputActionMap actionMap;
        readonly Dictionary<string, InputAction> inputs = new();


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

        public bool CanJump
        {
            get
            {
                return Attributes["stamina"].Value >= Attributes["jump_stamina_cost"].Value
                    && Controls.IsGrounded;
            }
        }

        public override void OnSpawn()
        {
            Initialize();
        }

        void Awake()
        {
            MainCamera = Camera.main;
            MoveStateMachine = GetComponent<MovementStateMachine>();
            ActionStateMachine = GetComponent<ActionStateMachine>();
            Controls = GetComponent<PlayerControls>();
            Actions = GetComponent<PlayerActions>();
            FPP = GetComponent<FPPController>();
        }

        void Initialize()
        {
            Game.Console.Log("I, player, has been spawned!");

            LoadDefaultsJson();
            /// this should be placed not here
            ReadSaveJson(saveData); /// currently loads default atm

            audioController.CreateAudioPool(8);
            audioController.LoadAudioAssetsData(playerEntityData.AudioAssetsData);

            InitializeAttributes();
            InitializeStateMachines();
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

        void LoadDefaultsJson()
        {
            saveData = playerEntityData.GetDefaultsJson<PlayerSaveData>();
        }


        #region Initializing methods

        void InitializeAttributes()
        {
            attributes.ReadSaveJson(saveData.Attributes);

            attributes["stamina"].OnValueModified += OnAttrStaminaModified;
        }

        void InitializeStateMachines()
        {
            MoveStateMachine.InitialState = MoveStateMachine.States[MoveStates.Idle];

            MoveStateMachine.OnStateChanged += OnMoveStateChanged;

            MoveStateMachine.States[MoveStates.Idle].OnEnter += OnIdleEnter;
            MoveStateMachine.States[MoveStates.Run].OnEnter += OnRunEnter;
            MoveStateMachine.States[MoveStates.Jump].OnEnter += OnJumpEnter;
            MoveStateMachine.States[MoveStates.Crouch].OnEnter += OnCrouchEnter;
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
            Camera.main.transform.SetParent(FPP.Camera.Holder, false);
            Camera.main.transform.localPosition = Vector3.zero;
        }

        #endregion

        
        void Tick(TickInfo t)
        {
            ConsumeStaminaWhileRunning();
            RegenerateStamina();
        }


        #region Attribute event callbacks

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

        void OnIdleEnter(object sender, State<MoveStates>.ChangedContext e)
        {
        }

        void OnRunEnter(object sender, State<MoveStates>.ChangedContext e)
        {
        }

        void OnCrouchEnter(object sender, State<MoveStates>.ChangedContext e)
        {
        }

        void OnJumpEnter(object sender, State<MoveStates>.ChangedContext e)
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


        #region Public methods

        public void ReadSaveJson(PlayerSaveData saveData)
        {
            if (saveData == null)
            {
                Game.Console.LogError($"Invalid PlayerSaveData loaded for Player");
                return;
            }
            this.saveData = saveData; 
        }
        
        public new PlayerSaveData WriteSaveJson()
        {
            var esd = base.WriteSaveJson();
            var psd = new PlayerSaveData
            {
                Id = esd.Id,
                Transform = esd.Transform,
                Attributes = attributes.WriteSaveJson(),
                Inventory = inventory.WriteSaveJson()
            };

            return psd;
        }

        public void UseObjectGUI(ObjectGUI gui)
        {
            invUI.AppendObjectGUI(gui, 1);
        }

        public void RemoveObjectGUI(ObjectGUI gui)
        {
            invUI.RemoveObjectGUI(gui);
        }

        #endregion


        void RegenerateStamina()
        {
            if (_allowStaminaRegen)
            {
                Attributes.Get("stamina").Add(Attributes.Get("stamina_regen_per_tick").Value);
            }
        }

        void ConsumeStaminaWhileRunning()
        {
            if (Controls.IsRunning && Controls.IsMoving)
            {
                /// Cache attributes for better performance
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