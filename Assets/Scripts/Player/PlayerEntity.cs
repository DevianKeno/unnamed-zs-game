using System;
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
    public class PlayerEntity : Entity, IAttributeCollection
    {
        public bool CanPickUpItems = true;

        #region Events
        public event EventHandler<EventArgs> OnDoneInit;
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
        [field: SerializeField] public AttributeCollection Attributes { get; private set; }
        public PlayerControls Controls { get; private set; }
        public PlayerActions Actions { get; private set; }
        public PlayerFPP FPP { get; private set; }
        public InventoryHandler Inventory { get; private set; }

        void Awake()
        {
            MainCamera = Camera.main;
            sm = GetComponent<StateMachine<PlayerStates>>();
            Controls = GetComponent<PlayerControls>();
            Actions = GetComponent<PlayerActions>();
            FPP = GetComponent<PlayerFPP>();
            // Inventory = GetComponent<InventoryHandler>();
        }

        public override void Spawn()
        {
            base.Spawn();
            Initialize();
        }

        void Initialize()
        {
            Game.Console.Log("I, player, has been spawned!");

            InitializeAttributes();
            Controls.Initialize();
            Actions.Initialize();
            FPP.Initialize();

            sm.InitialState = sm.States[PlayerStates.Idle];

            sm.States[PlayerStates.Idle].OnEnter += OnIdleX;
            sm.States[PlayerStates.Run].OnEnter += OnRunX;
            sm.States[PlayerStates.Jump].OnEnter += OnJumpX;
            sm.States[PlayerStates.Crouch].OnEnter += OnCrouchX;

            OnDoneInit?.Invoke(this, new());
            Game.Tick.OnTick += Tick;
        }

        void InitializeAttributes()
        {
            var attr = Game.AttributesManager.CreateAttribute("health");
            Attributes.AddAttribute(attr);
            
            attr = Game.AttributesManager.CreateAttribute("stamina");
            Attributes.AddAttribute(attr);
            
            attr = Game.AttributesManager.CreateAttribute("mana");
            Attributes.AddAttribute(attr);

            attr = Game.AttributesManager.CreateAttribute("hunger");
            Attributes.AddAttribute(attr);

            attr = Game.AttributesManager.CreateAttribute("hydration");
            Attributes.AddAttribute(attr);
            
            attr = Game.AttributesManager.CreateAttribute("move_speed");
            attr.Value = 7f;
            Attributes.AddAttribute(attr);

            attr = Game.AttributesManager.CreateAttribute("run_speed");
            attr.Value = Attributes.GetAttributeFromId("move_speed").Value * 1.5f;
            Attributes.AddAttribute(attr);

            attr = Game.AttributesManager.CreateAttribute("crouch_speed");
            attr.Value = Attributes.GetAttributeFromId("move_speed").Value * 0.4f;
            Attributes.AddAttribute(attr);
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
            float jumpStaminaCost = 10f;
            Attributes.GetAttributeFromId("stamina").Remove(jumpStaminaCost);
        }
    }
}