using System;
using UnityEngine;
using UZSG.Systems;
using UZSG.Inventory;
using UZSG.Attributes;
using UZSG.PlayerCore;
using UZSG.Data;
using UZSG.FPP;

namespace UZSG.Entities
{
    /// <summary>
    /// Player entity.
    /// </summary>
    public class Player : Entity, IStateMachine<PlayerStates>
    {
        public bool CanPickUpItems = true;
        
        [field: Header("Flags")]
        public PlayerData Data { get; private set; }
        public override EntityData EntityData { get => playerEntityData; }
        [SerializeField] PlayerEntityData playerEntityData;
        [field: SerializeField] public AttributeCollection<VitalAttribute> Vitals { get; private set; }
        [field: SerializeField] public AttributeCollection<GenericAttribute> Generic { get; private set; }
        [SerializeField] InventoryHandler _inventory;
        public InventoryHandler Inventory => _inventory;

        #region Events
        public event EventHandler<EventArgs> OnDoneInit;
        #endregion

        [field: Header("Components")]
        /// <summary>
        /// Unity camera tagged "MainCamera"
        /// </summary>
        public Camera MainCamera { get; private set; }
        StateMachine<PlayerStates> _stateMachine;
        /// <summary>
        /// Player state machine.
        /// </summary>
        public StateMachine<PlayerStates> sm => _stateMachine;
        public PlayerControls Controls { get; private set; }
        public PlayerActions Actions { get; private set; }
        public PlayerFPP FPP { get; private set; }

        public override void OnSpawn()
        {
            Init();
        }

        void Awake()
        {
            MainCamera = Camera.main;
            _stateMachine = GetComponent<StateMachine<PlayerStates>>();
            Controls = GetComponent<PlayerControls>();
            Actions = GetComponent<PlayerActions>();
            FPP = GetComponent<PlayerFPP>();
        }

        void Init()
        {
            Game.Console.Log("I, player, has been spawned!");

            // Load PlayerData from file
            // Data.LoadFromFile("/path");

            InitAttributes();
            InitInventory();
            InitStateMachine();
            
            Controls.Init();
            Actions.Init();
            FPP.Init();
            
            Game.UI.HUD.BindPlayer(this);
            Game.UI.HUD.ToggleVisibility(true);
            Game.Tick.OnTick += Tick;
            
            OnDoneInit?.Invoke(this, new());
        }

        void InitAttributes()
        {
            // Get blueprint base
            Vitals = new(playerEntityData.Vitals);

            // Overwrite base with data from file
            // Vitals.LoadData(Data.Vitals);

            // Initialize
            Vitals.Init();

            Generic = new(playerEntityData.Generic);
            // Generic.LoadData(Data.Generic);
            Generic.Init();

            #region Temporary
            // These should be read from file
            // e.g. Attributes.LoadData();
            #endregion
        }

        void InitInventory()
        {
            // Inventory.LoadData(Data.Inventory);

            Inventory.Bag.SlotsCount = (int) Generic.GetAttributeFromId("pockets_size").Value;
            Inventory.Hotbar.SlotsCount = (int) Generic.GetAttributeFromId("hotbar_size").Value;
            Inventory.Init();

            Game.UI.InitInventoryWindow(_inventory);
        }
        
        void InitStateMachine()
        {
            sm.InitialState = sm.States[PlayerStates.Idle];

            sm.States[PlayerStates.Idle].OnEnter += OnIdleEnter;
            sm.States[PlayerStates.Run].OnEnter += OnRunEnter;
            sm.States[PlayerStates.Jump].OnEnter += OnJumpEnter;
            sm.States[PlayerStates.Crouch].OnEnter += OnCrouchEnter;
        }

        void Tick(object sender, TickEventArgs e)
        {
        }

        void OnIdleEnter(object sender, State<PlayerStates>.ChangedContext e)
        {
        }

        void OnRunEnter(object sender, State<PlayerStates>.ChangedContext e)
        {
        }

        void OnCrouchEnter(object sender, State<PlayerStates>.ChangedContext e)
        {
        }

        void OnJumpEnter(object sender, State<PlayerStates>.ChangedContext e)
        {
            if (Vitals.TryGetAttributeFromId("stamina", out Attributes.Attribute attr))
            {
                float jumpStaminaCost = Generic.GetAttributeFromId("jump_stamina_cost").Value;
                attr.Remove(jumpStaminaCost);
            }
        }
    }
}