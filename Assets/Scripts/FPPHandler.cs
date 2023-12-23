using System;
using UnityEngine;
using URMG.Player;
using URMG.UI;
using URMG.States;

namespace URMG
{
    // public class StartEquipState : State
    // {
    // }

    // public class EquippedState : State
    // {
    // }
    
    // public class PerformPrimaryState : State
    // {
    // }
    
    // public class PerformSecondaryState : State
    // {
    // }

    // public class DeequipState : State
    // {
    // }

    public struct EquipableArgs
    {
    }

    public interface IEquipable
    {
    }

    /// <summary>
    /// Handles the Equipable items of the first-person view.
    /// </summary>
    [RequireComponent(typeof(PlayerActions))]
    public class FPPHandler : MonoBehaviour
    {
        [SerializeField] Camera _camera;
        PlayerActions _playerActions;
        Animator _animator;
        FPPStateMachine _stateMachine;

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

        public void Equip(IEquipable obj)
        {
        }
    }

    public class FPPStateMachine : StateMachine
    {
        
    }
}
