using System;
using System.Collections.Generic;
using System.IO;

using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

using UZSG.Systems;
using UZSG.Inventory;
using UZSG.Interactions;
using UZSG.Attributes;
using UZSG.Players;
using UZSG.Data;
using UZSG.FPP;
using UZSG.UI;
using UZSG.StatusEffects;
using UZSG.Crafting;

namespace UZSG.Entities
{
    /// <summary>
    /// Player entity.
    /// </summary>
    public class Player : Entity, IAttributable, IInteractActor
    {
        public bool CanPickUpItems = true;

        [SerializeField] PlayerSaveData playerData;
        public PlayerSaveData PlayerData => playerData;
        [SerializeField] PlayerEntityData playerEntityData;
        public PlayerEntityData PlayerEntityData => playerEntityData;
        [SerializeField] AttributeCollection<VitalAttribute> vitals;
        public AttributeCollection<VitalAttribute> Vitals => vitals;
        [SerializeField] AttributeCollection<GenericAttribute> generic;
        public AttributeCollection<GenericAttribute> Generic => generic;
        [SerializeField] AttributeCollection<Attributes.Attribute> attributes;
        public AttributeCollection<Attributes.Attribute> Attributes => attributes;
        [SerializeField] InventoryHandler inventory;
        public InventoryHandler Inventory => inventory;
        [SerializeField] PlayerCrafting craftingAgent;
        public Crafter CraftingAgent => craftingAgent;
        StatusEffectCollection statusEffects;
        public StatusEffectCollection StatusEffects => statusEffects;
        [SerializeField] AudioSourceController audioController;
        
        public Vector3 Forward => MainCamera.transform.forward;
        public Vector3 EyeLevel => MainCamera.transform.position;

        PlayerInventoryWindow invUI;
        public PlayerInventoryWindow InventoryGUI => invUI;
        PlayerHUD _HUD;
        public PlayerHUD HUD => _HUD;

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

        /// <summary>
        /// All Player Data
        /// </summary>
        const string playerDefaultsPath = "/Resources/Defaults/Entities/player_defaults.json";
        public PlayerSaveData saveData;

        public bool CanJump
        {
            get
            {
                return Vitals.Get("stamina").Value >= Generic.Get("jump_stamina_cost").Value && Controls.IsGrounded;
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

            LoadDefaults();
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

        void LoadDefaults()
        {
            var defaultsJson = File.ReadAllText(Application.dataPath + playerDefaultsPath);
            saveData = JsonUtility.FromJson<PlayerSaveData>(defaultsJson);
        }

        void InitializeAttributes()
        {
            vitals = new();
            vitals.ReadSaveJSON(saveData.VitalAttributes);

            generic = new();
            generic.ReadSaveJSON(saveData.GenericAttributes);
        }
        
        void InitializeStateMachines()
        {
            MoveStateMachine.InitialState = MoveStateMachine.States[MoveStates.Idle];

            MoveStateMachine.States[MoveStates.Idle].OnEnter += OnIdleEnter;
            MoveStateMachine.States[MoveStates.Run].OnEnter += OnRunEnter;
            MoveStateMachine.States[MoveStates.Jump].OnEnter += OnJumpEnter;
            MoveStateMachine.States[MoveStates.Crouch].OnEnter += OnCrouchEnter;      
        }

        void InitializeInventory()
        {
            // Inventory.LoadData(Data.Inventory);
            inventory.Initialize();

            invUI = Game.UI.Create<PlayerInventoryWindow>("Player Inventory", show: false);
            invUI.BindPlayer(this);
            invUI.Initialize();
            invUI.OnOpen += () =>
            {
                Actions.ActionMap.Disable();
                inputs["Look"].Disable();
            };
            invUI.OnClose += () =>
            {
                Actions.ActionMap.Enable();
                inputs["Look"].Enable();
            };
        }

        void InitializeCrafter()
        {
            craftingAgent.InitializePlayer(this);
            // craftingAgent.AddContainer(inventory.Bag);
            // craftingAgent.AddContainer(inventory.Hotbar);
        }

        void InitializeHUD()
        {
            _HUD = Game.UI.Create<PlayerHUD>("Player HUD");
            _HUD.BindPlayer(this);
            _HUD.Initialize();
        }

        InputActionMap actionMap;
        readonly Dictionary<string, InputAction> inputs = new();

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
            Camera.main.transform.SetParent(FPP.CameraController.Camera.transform, false);
            Camera.main.transform.localPosition = Vector3.zero;
        }

        void Tick(TickInfo t)
        {
            /// Consume stamina while running
            if (Controls.IsRunning && Controls.IsMoving)
            {
                /// Cache attributes for better performance
                var runStaminaCost = Generic.Get("run_stamina_cost").Value;
                Vitals.Get("stamina").Remove(runStaminaCost);
            }
        }

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
            if (Vitals.TryGet("stamina", out var stamina))
            {
                float jumpStaminaCost = Generic.Get("jump_stamina_cost").Value;
                stamina.Remove(jumpStaminaCost);
            }
            /// Consume Hunger on jump
            if (Vitals.TryGet("hunger", out var hunger))
            {
                float jumpHungerCost = Generic.Get("jump_hunger_cost").Value;
                hunger.Remove(jumpHungerCost);
            }
            /// Consume Hydration on jump
            if (Vitals.TryGet("hydration", out var hydration))
            {
                float jumpHydrationCost = Generic.Get("jump_hydration_cost").Value;
                hydration.Remove(jumpHydrationCost);
            }
        }
        
        /// <summary>
        /// I want the cam to lock and cursor to appear only when the key is released :P
        /// </summary>    
        void OnPerformInventory(InputAction.CallbackContext context)
        {
            ToggleInventory();
        }

        public void ToggleInventory()
        {
            invUI.ToggleVisibility();
            if (invUI.IsVisible)
            {
                HUD.Hide();
                invUI.OnClose += () =>
                {
                    HUD.Show();
                };
            }
        }
    }
}