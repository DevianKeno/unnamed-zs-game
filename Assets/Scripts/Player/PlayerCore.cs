using System;
using UnityEngine;
using UZSG.Systems;
using UZSG.Inventory;

namespace UZSG.Player
{
    /// <summary>
    /// Player core functionalities.
    /// This contains all information related to the Player.
    /// </summary>
    public class PlayerCore : MonoBehaviour
    {
        public bool CanPickUpItems = true;
        bool _isSpawned = false;
        bool _isAlive = false;
        
        [SerializeField] PlayerStateMachine _sm;
        /// <summary>
        /// Player state machine.
        /// </summary>
        public PlayerStateMachine sm => _sm;
        [SerializeField] InventoryHandler _inventory;
        public InventoryHandler Inventory { get => _inventory; }

        void Awake()
        {
            _sm = GetComponent<PlayerStateMachine>();
        }

        void Start()
        {
            _sm.InitialState = _sm.States[PlayerStates.Idle];
            _sm.States[PlayerStates.Jump].OnEnter += OnJumpX;
            _sm.States[PlayerStates.Crouch].OnEnter += OnCrouchX;
        }

        void OnCrouchX(object sender, State<PlayerStates>.ChangedContext e)
        {
            Debug.Log("uy yuko");
        }

        void OnJumpX(object sender, State<PlayerStates>.ChangedContext e)
        {
            Debug.Log("haha talonskie");
        }
    }
}