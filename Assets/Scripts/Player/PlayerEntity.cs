using System;
using System.Collections.Generic;
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

        [Tooltip("State machine.")]
        [SerializeField] StateMachine<PlayerStates> _sm;
        /// <summary>
        /// Player state machine.
        /// </summary>
        public StateMachine<PlayerStates> sm => _sm;
        [SerializeField] PlayerControls _controls;

        [SerializeField] AttributeCollection _attributes;
        public AttributeCollection Attributes => _attributes;

        [SerializeField] InventoryHandler _inventory;
        public InventoryHandler Inventory => _inventory;

        public event EventHandler<EventArgs> OnDoneInit;
        public Camera MainCamera;

        void Awake()
        {
            _sm = GetComponent<StateMachine<PlayerStates>>();
            _controls.GetComponent<PlayerControls>();
            MainCamera = Camera.main;
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
            _controls.Initialize();

            _sm.InitialState = _sm.States[PlayerStates.Idle];

            _sm.States[PlayerStates.Idle].OnEnter += OnIdleX;
            _sm.States[PlayerStates.Run].OnEnter += OnRunX;
            _sm.States[PlayerStates.Jump].OnEnter += OnJumpX;
            _sm.States[PlayerStates.Crouch].OnEnter += OnCrouchX;

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
            _attributes.GetAttributeFromId("stamina").Remove(jumpStaminaCost);
        }
    }
}