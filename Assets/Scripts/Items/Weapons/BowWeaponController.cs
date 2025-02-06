using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.InputSystem;


using UZSG.Data;
using UZSG.Entities;
using UZSG.Players;

namespace UZSG.Items.Weapons
{
    public class BowWeaponController : HeldWeaponController, IReloadable
    {
        public Player Player => owner as Player;

        bool _isHoldingLeftClick;
        bool _isLoaded;
        float _chargeLevel;

        [Space]
        [SerializeField] BowWeaponStateMachine stateMachine;
        public BowWeaponStateMachine StateMachine => stateMachine;

        public override void Initialize()
        {
            InitializeEventsFromOwnerInput();
        }

        void InitializeEventsFromOwnerInput()
        {
            if (owner is not Player player) return;
            var inputs = player.Actions.Inputs;

            inputs["Primary Action"].started += OnPlayerPrimary;
            inputs["Primary Action"].canceled += OnPlayerPrimary;

            inputs["Secondary Action"].started += OnPlayerSecondary;
            inputs["Secondary Action"].canceled += OnPlayerSecondary;

            inputs["Reload"].performed += OnPlayerReload;
        }


        #region Player input callbacks

        void OnPlayerPrimary(InputAction.CallbackContext context)
        {
            if (context.started)
            {
                _isHoldingLeftClick = true;
                if (_isLoaded)
                {
                    StartCoroutine(StartCharge());
                }
            }
            else
            {
                _isHoldingLeftClick = false;
            }
        }

        void OnPlayerSecondary(InputAction.CallbackContext context)
        {
            
        }

        void OnPlayerReload(InputAction.CallbackContext context)
        {
            
        }

        #endregion


        IEnumerator StartCharge()
        {
            _chargeLevel = 0f;
            stateMachine.ToState(BowWeaponStates.Draw);

            while (_isHoldingLeftClick)
            {
                _chargeLevel += Time.deltaTime;
            }

            TryFire();
            yield break;
        }

        void TryFire()
        {
            if (!_isLoaded) return; /// lewl?


        }

        void SpawnArrowProjectile()
        {            
            // Game.Entity.Spawn<Arrow>("arrow", callback: (info) =>
            // {
                
            // });
        }

        
        public bool TryReload(float durationSeconds)
        {
            if (_isLoaded) return false;

            stateMachine.ToState(BowWeaponStates.Reload);

            _isLoaded = true;

            return true;
        }

        public override void SetStateFromAction(ActionStates state)
        {
            throw new NotImplementedException();
        }
    }
}
