using System;
using UnityEngine;

using UZSG.Players;

namespace UZSG.Items.Weapons
{
    public class MeleeWeapon : WeaponEquipped
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

        public void SetWeaponStateFromPlayerAction(ActionStates state)
        {
            throw new NotImplementedException();
        }
    }
}