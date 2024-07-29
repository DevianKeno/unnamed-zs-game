using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UZSG.Entities;
using UZSG.Players;

namespace UZSG.Items.Weapons
{
    public class MeleeWeaponController : WeaponController
    {
        bool _inhibitActions;
        bool _isAttacking;

        MeleeWeaponStateMachine stateMachine;
        public MeleeWeaponStateMachine StateMachine => stateMachine;

        void Awake()
        {
            stateMachine = GetComponent<MeleeWeaponStateMachine>();
        }

        public override void Initialize()
        {
            InitializeEventsFromOwnerInput();
        }

        void InitializeEventsFromOwnerInput()
        {
            if (owner is not Player player) return;
            actionMap = player.Controls.ActionMap; /// DISBALE ALL ACTIONS ON DISABLE HELD ITEM
            var inputs = player.Controls.Inputs;

            inputs["Primary Action"].started += OnPlayerPrimary;
            inputs["Primary Action"].canceled += OnPlayerPrimary;

            inputs["Secondary Action"].started += OnPlayerSecondary;
            inputs["Secondary Action"].canceled += OnPlayerSecondary;
        }


        #region Player input callbacks

        void OnPlayerPrimary(InputAction.CallbackContext context)
        {
            if (!gameObject.activeSelf) return;
            
            if (context.started)
            {
                StartCoroutine(AttackCoroutine());
            }
            else if (context.canceled)
            {
                
            }
        }

        IEnumerator AttackCoroutine()
        {
            if (_inhibitActions || _isAttacking)
            {
                yield break;
            }
            
            _inhibitActions = true;
            _isAttacking = true;
            
            CreateAttackHitbox();
            stateMachine.ToState(MeleeWeaponStates.Attack);
            yield return new WaitForSeconds(0.5f); /// SOMETHING ATKSPD THOUGH NOT SO STRAIGHFROWARDS LOTS OF CALCS (short for calculations)
        }

        void OnPlayerSecondary(InputAction.CallbackContext context)
        {
            
            if (context.started)
            {
                
            }
            else if (context.canceled)
            {
                
            }
        }

        #endregion 

        public override void SetStateFromAction(ActionStates state)
        {
            
        }

        void CreateAttackHitbox()
        {

        }
    }
}