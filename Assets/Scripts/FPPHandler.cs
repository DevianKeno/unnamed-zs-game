using System;
using UnityEngine;
using UZSG.Player;
using UZSG.UI;
using UZSG.States;
using UZSG.Interactions;

namespace UZSG
{
    public struct EquipableArgs
    {
    }

    /// <summary>
    /// Stuff that are visible in FPP when equipped.
    /// </summary>
    public interface IFPPVisible
    {
        public GameObject FPPModel { get; }
    }

    /// <summary>
    /// Handles the Equipable items of the first-person view.
    /// </summary>
    [RequireComponent(typeof(PlayerActions))]
    public class FPPHandler : MonoBehaviour
    {
        public class EquipState : State
        {
            public override string Name => "Equip";
        }

        public class IdleState : State
        {
            public override string Name => "Idle";
        }
        
        public class PerformPrimaryState : State
        {
            public override string Name => "Primary";
        }
        
        public class PerformSecondaryState : State
        {
            public override string Name => "Secondary";
        }

        public class DequipState : State
        {
            public override string Name => "Dequip";
        }

        [SerializeField] Camera _camera;
        PlayerActions _playerActions;
        Animator _animator;
        FPPStateMachine _stateMachine;

        [SerializeField] GameObject katana;

        void Awake()
        {
            _playerActions = GetComponent<PlayerActions>();
        }

        void Start()
        {    
            UI.Cursor.Hide();
            _playerActions.OnActionPerform += ActionPerformed;
        }

        void ActionPerformed(object sender, PlayerActions.ActionPerformedArgs e)
        {
            if (e.Action == PlayerActions.Actions.SelectHotbar)
            {

            }
        }

        public void Equip(IFPPVisible obj)
        {
            Instantiate(obj.FPPModel, _camera.transform);
        }
    }

    public class FPPStateMachine : StateMachine
    {
        
    }
}
