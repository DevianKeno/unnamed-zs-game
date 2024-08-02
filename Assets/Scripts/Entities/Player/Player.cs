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
using UZSG.Crafting;

namespace UZSG.Entities
{
    public interface IInteractActor
    {
    }

    /// <summary>
    /// Player entity.
    /// </summary>
    public class Player : Entity, IInteractActor
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
        [SerializeField] Crafter craftingAgent;
        public Crafter CraftingAgent => craftingAgent;
        StatusEffectCollection statusEffects;
        public StatusEffectCollection StatusEffects => statusEffects;
        
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
            Controls.Enable();
            Actions.Initialize();
            Actions.Enable();
            
            FPP.Initialize();
            ParentMainCameraToFPPController();

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
            inventory.Bag.SlotsCount = (int) Generic.GetAttribute("bag_slots_count").Value;
            inventory.Hotbar.SlotsCount = (int) Generic.GetAttribute("hotbar_size").Value;
            inventory.Initialize();

            invUI = Game.UI.Create<PlayerInventoryWindow>("Player Inventory", show: false);
            invUI.BindPlayer(this);
            invUI.Initialize();
            invUI.OnOpen += () =>
            {
                ToggleControlsOnGUI(false);
            };
            invUI.OnClose += () =>
            {
                ToggleControlsOnGUI(true);
            };
        }

        void InitializeHUD()
        {
            _HUD = Game.UI.Create<PlayerHUD>("Player HUD");
            _HUD.BindPlayer(this);
            _HUD.Initialize();
        }
        
        void InitializeStateMachines()
        {
            MoveStateMachine.InitialState = MoveStateMachine.States[MoveStates.Idle];

            MoveStateMachine.States[MoveStates.Idle].OnEnter += OnIdleEnter;
            MoveStateMachine.States[MoveStates.Run].OnEnter += OnRunEnter;
            MoveStateMachine.States[MoveStates.Jump].OnEnter += OnJumpEnter;
            MoveStateMachine.States[MoveStates.Crouch].OnEnter += OnCrouchEnter;      
        }

        void ParentMainCameraToFPPController()
        {
            Camera.main.transform.SetParent(FPP.CameraController.Camera.transform, false);
            Camera.main.transform.localPosition = Vector3.zero;
        }

        void Tick(TickInfo e)
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
            if (Vitals.TryGetAttribute("stamina", out Attributes.Attribute attr))
            {
                float jumpStaminaCost = Generic.GetAttribute("jump_stamina_cost").Value;
                attr.Remove(jumpStaminaCost, buffer: true);
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

        public void ToggleControlsOnGUI(bool enable)
        {
            Controls.SetControl("Look", enable);
            Controls.SetControl("Primary Action", enable);
            Controls.SetControl("Secondary Action", enable);
            Controls.SetControl("Reload", enable);
            Controls.SetControl("Hotbar", enable);
            Controls.SetControl("Interact", enable);
            Controls.SetControl("Unholster", enable);
        }
    }
}