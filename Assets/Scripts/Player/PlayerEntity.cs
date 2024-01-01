using System;
using UnityEngine;
using UZSG.Systems;
using UZSG.Inventory;
using UZSG.Entities;

namespace UZSG.Player
{
    /// <summary>
    /// Player core functionalities.
    /// This contains all information related to the Player.
    /// </summary>
    public class PlayerEntity : Entity
    {
        public bool CanPickUpItems = true;
        bool _isSpawned = false;
        bool _isAlive = false;
        
        [Tooltip("State machine.")]
        [SerializeField] StateMachine<PlayerStates> _sm;
        /// <summary>
        /// Player state machine.
        /// </summary>
        public StateMachine<PlayerStates> sm => _sm;
        [SerializeField] PlayerAttributes _attributes;
        public PlayerAttributes Attributes => _attributes;
        [SerializeField] InventoryHandler _inventory;
        public InventoryHandler Inventory => _inventory;

        public float Stamina;

        public event EventHandler<EventArgs> OnDoneInit;

        void Awake()
        {
            _sm = GetComponent<StateMachine<PlayerStates>>();
            _attributes = GetComponent<PlayerAttributes>();
        }        

        public override void Spawn(Vector3 position)
        {
            base.Spawn(position);

            Initialize();
        }

        void Initialize()
        {
            _attributes.Initialize();
            
            _sm.InitialState = _sm.States[PlayerStates.Idle];

            _sm.States[PlayerStates.Idle].OnEnter += OnIdleX;
            _sm.States[PlayerStates.Run].OnEnter += OnRunX;
            _sm.States[PlayerStates.Jump].OnEnter += OnJumpX;
            _sm.States[PlayerStates.Crouch].OnEnter += OnCrouchX;

            OnDoneInit?.Invoke(this, new());
            Game.Tick.OnTick += Tick;
        }

        void Tick(object sender, TickEventArgs e)
        {
            Stamina = _attributes.Vital["Stamina"].Value;
            Debug.Log(Stamina);
        }

        void OnIdleX(object sender, State<PlayerStates>.ChangedContext e)
        {
        }

        void OnRunX(object sender, State<PlayerStates>.ChangedContext e)
        {
        }

        void OnCrouchX(object sender, State<PlayerStates>.ChangedContext e)
        {
            Debug.Log("uy yuko");
        }

        void OnJumpX(object sender, State<PlayerStates>.ChangedContext e)
        {
            float jumpStaminaCost = 10f;
            _attributes.Vital["Stamina"].Remove(jumpStaminaCost);
        }
    }
}