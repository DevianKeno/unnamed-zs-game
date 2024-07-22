using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

using UZSG.Systems;
using UZSG.Inventory;
using UZSG.Attributes;
using UZSG.Players;
using UZSG.Data;
using UZSG.FPP;
using UZSG.UI;
using UZSG.StatusEffects;

namespace UZSG.Entities
{
    /// <summary>
    /// Player entity.
    /// </summary>
    public class Player : Entity
    {
        public bool CanPickUpItems = true;

        [SerializeField] PlayerData playerData;
        public PlayerData PlayerData => playerData;
        [SerializeField] PlayerEntityData playerEntityData;
        public PlayerEntityData PlayerEntityData => playerEntityData;
        [SerializeField] AttributeCollection<VitalAttribute> vitals;
        public AttributeCollection<VitalAttribute> Vitals => vitals;
        [SerializeField] AttributeCollection<GenericAttribute> generic;
        public AttributeCollection<GenericAttribute> Generic => generic;
        [SerializeField] InventoryHandler inventory;
        public InventoryHandler Inventory => inventory;
        StatusEffectCollection statusEffects;
        public StatusEffectCollection StatusEffects => statusEffects;
        
        public Vector3 Forward => MainCamera.transform.forward;
        public Vector3 EyeLevel => MainCamera.transform.position;

        PlayerInventoryWindow invUI;
        PlayerHUD HUD;
        

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

            // Load PlayerData from file
            // Data.LoadFromFile("/path");

            InitializeAttributes();
            InitializeStateMachines();
            InitializeInventory();
            InitializeHUD();
            InitializeInputs();
            
            Controls.Initialize();
            Actions.Initialize();
            FPP.Initialize();
            
            // Game.UI.HUD.BindPlayer(this);
            // Game.UI.HUD.ToggleVisibility(true);
            Game.Tick.OnTick += Tick;
            
            OnDoneInit?.Invoke(this, new());
        }

        InputActionMap actionMap;
        Dictionary<string, InputAction> inputs = new();

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

        void InitializeAttributes()
        {
            /// Get blueprint base
            vitals = new(playerEntityData.Vitals);

            /// Overwrite base with data from file
            // Vitals.LoadData(Data.Vitals);

            /// Initialize
            vitals.Init();

            generic = new(playerEntityData.Generic);
            // Generic.LoadData(Data.Generic);
            generic.Init();

#region Temporary
            /// These should be read from file
            /// e.g. Attributes.LoadData();
#endregion
        }

        void InitializeInventory()
        {
            // Inventory.LoadData(Data.Inventory);
            inventory.Bag.SlotsCount = (int) Generic.GetAttributeFromId("bag_slots_count").Value;
            inventory.Hotbar.SlotsCount = (int) Generic.GetAttributeFromId("hotbar_size").Value;
            inventory.Initialize();

            invUI = Game.UI.Create<PlayerInventoryWindow>("player_inventory");
            invUI.BindPlayer(this);
            invUI.Initialize();
            invUI.OnOpen += () =>
            {
                TogglePlayerControlsOnInventory(false);
            };
            invUI.OnClose += () =>
            {
                TogglePlayerControlsOnInventory(true);
            };
        }

        void InitializeHUD()
        {
            HUD = Game.UI.Create<PlayerHUD>("player_hud");
            HUD.BindPlayer(this);
            HUD.Initialize();
        }
        
        void InitializeStateMachines()
        {
            MoveStateMachine.InitialState = MoveStateMachine.States[MoveStates.Idle];

            MoveStateMachine.States[MoveStates.Idle].OnEnter += OnIdleEnter;
            MoveStateMachine.States[MoveStates.Run].OnEnter += OnRunEnter;
            MoveStateMachine.States[MoveStates.Jump].OnEnter += OnJumpEnter;
            MoveStateMachine.States[MoveStates.Crouch].OnEnter += OnCrouchEnter;      
        }

        void Tick(object sender, TickEventArgs e)
        {
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
            if (Vitals.TryGetAttributeFromId("stamina", out Attributes.Attribute attr))
            {
                float jumpStaminaCost = Generic.GetAttributeFromId("jump_stamina_cost").Value;
                attr.Remove(jumpStaminaCost);
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
        }

        void TogglePlayerControlsOnInventory(bool enable)
        {
            Controls.SetControl("Look", enable);
            Controls.SetControl("Primary Action", enable);
            Controls.SetControl("Secondary Action", enable);
            Controls.SetControl("Reload", enable);
            Controls.SetControl("Hotbar", enable);
            Controls.SetControl("Unholster", enable);
        }
    }
}