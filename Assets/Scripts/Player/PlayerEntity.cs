using UnityEngine;
using UZSG.Systems;
using UZSG.Inventory;
using UZSG.Entities;
using UZSG.Attributes;

namespace UZSG.Player
{
    /// <summary>
    /// Player core functionalities.
    /// This contains all information related to the Player.
    /// </summary>
    public class PlayerEntity : Entity
    {
        public bool CanPickUpItems = true;
        public override EntityData Data { get => playerEntityData; }
        [SerializeField] PlayerEntityData playerEntityData;

        #region Events
        public event System.EventHandler<System.EventArgs> OnDoneInit;
        #endregion

        [field: Header("Components")]
        /// <summary>
        /// The Unity camera tagged "MainCamera"
        /// </summary>
        public Camera MainCamera { get; private set; }
        /// <summary>
        /// Player state machine.
        /// </summary>
        public StateMachine<PlayerStates> sm { get; private set; }
        [field: SerializeField] public AttributeCollection<VitalAttribute> Vitals { get; private set; }
        [field: SerializeField] public AttributeCollection<GenericAttribute> Generic { get; private set; }
        public PlayerControls Controls { get; private set; }
        public PlayerActions Actions { get; private set; }
        public PlayerFPP FPP { get; private set; }
        [SerializeField] InventoryHandler _inventory;
        public InventoryHandler Inventory => _inventory;

        void Awake()
        {
            MainCamera = Camera.main;
            sm = GetComponent<StateMachine<PlayerStates>>();
            Controls = GetComponent<PlayerControls>();
            Actions = GetComponent<PlayerActions>();
            FPP = GetComponent<PlayerFPP>();
        }

        public override void OnSpawn()
        {
            Initialize();
        }

        void Initialize()
        {
            Game.Console.Log("I, player, has been spawned!");
            
            InitializeAttributes();
            InitializeInventory();
            
            Controls.Initialize();
            Actions.Initialize();
            FPP.Initialize();
            
            sm.InitialState = sm.States[PlayerStates.Idle];

            sm.States[PlayerStates.Idle].OnEnter += OnIdleX;
            sm.States[PlayerStates.Run].OnEnter += OnRunX;
            sm.States[PlayerStates.Jump].OnEnter += OnJumpX;
            sm.States[PlayerStates.Crouch].OnEnter += OnCrouchX;

            Game.UI.HUD.BindPlayer(this);
            Game.UI.HUD.ToggleVisibility(true);
            Game.Tick.OnTick += Tick;
            
            OnDoneInit?.Invoke(this, new());
        }

        void InitializeAttributes()
        {
            Vitals = new(playerEntityData.Vitals);
            Generic = new(playerEntityData.Generic);
            Vitals.Initialize();
            Generic.Initialize();

            #region Temporary
            // These should be read from file
            // e.g. Attributes.LoadData();
            #endregion
        }

        void InitializeInventory()
        {
            Inventory.Bag.SlotsCount = (int) Generic.GetAttributeFromId("pockets_size").Value;
            Inventory.Hotbar.SlotsCount = (int) Generic.GetAttributeFromId("hotbar_size").Value;
            Inventory.Initialize();

            Game.UI.InitializeInventoryWindow(_inventory);
        }

        void Tick(object sender, TickEventArgs e)
        {
        }

        void OnIdleX(object sender, State<PlayerStates>.ChangedContext e)
        {
        }

        void OnRunX(object sender, State<PlayerStates>.ChangedContext e)
        {
        }

        void OnCrouchX(object sender, State<PlayerStates>.ChangedContext e)
        {
        }

        void OnJumpX(object sender, State<PlayerStates>.ChangedContext e)
        {
            Debug.Log("jumped");
            if (Vitals.TryGetAttributeFromId("stamina", out Attribute attr))
            {
                float jumpStaminaCost = Generic.GetAttributeFromId("jump_stamina_cost").Value;
                attr.Remove(jumpStaminaCost);
            }
        }
    }
}