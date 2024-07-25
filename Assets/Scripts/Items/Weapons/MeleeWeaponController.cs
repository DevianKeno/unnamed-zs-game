using System;
using UnityEngine;

using UZSG.Players;

namespace UZSG.Items.Weapons
{
    public class MeleeWeaponController : WeaponController
    {
        MeleeWeaponStateMachine stateMachine;
        public MeleeWeaponStateMachine StateMachine => stateMachine;

        void Awake()
        {
            stateMachine = GetComponent<MeleeWeaponStateMachine>();
        }

        public override void Initialize()
        {
            throw new NotImplementedException();
        }

        public override void SetStateFromAction(ActionStates state)
        {
            throw new NotImplementedException();
        }
    }
}