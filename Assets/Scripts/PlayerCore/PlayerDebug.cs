using System;
using TMPro;
using UnityEngine;
using UZSG.Entities;

using UZSG.UI;

namespace UZSG.Players
{
    public class PlayerDebug : MonoBehaviour
    {
        public Player Player;
        public PlayerDebugWindow UI;
        [SerializeField] GameObject UIPrefab;

        void Awake()
        {
            Player.OnDoneInit += Init;
        }

        void Init(Player player)
        {
            UI = Instantiate(UIPrefab, Game.UI.Canvas.transform).GetComponent<PlayerDebugWindow>();
            UI.Hide();

            Player.MoveStateMachine.OnTransition += MovementStateChanged;
            Player.ActionStateMachine.OnTransition += ActionStateChanged;
        }

        void FixedUpdate()
        {/*
            UI.physicsText.text = $@"TPS: {Game.Tick.TPS}
SPT: {Game.Tick.SecondsPerTick}
Delta time: {Time.deltaTime}
Player movement physics:
  currentV: {Player.Controls.Rigidbody.velocity}
  targetV: {Player.Controls._targetVelocity}
  deltaMultiplier: {Player.Controls.deltaTargetDebug}
  changeV: {Player.Controls._velocityChange}
";*/
        }

        void MovementStateChanged(StateMachine<MoveStates>.TransitionContext e)
        {
            UI.movementStateText.text = $"Move state: {e.To}";
        }

        void ActionStateChanged(StateMachine<ActionStates>.TransitionContext e)
        {
            UI.actionStateText.text = $"Action state: {e.To}";
        }

        void OnDisable()
        {
            Player.OnDoneInit -= Init;
            Player.MoveStateMachine.OnTransition -= MovementStateChanged;
            Player.ActionStateMachine.OnTransition -= ActionStateChanged;
        }
    }
}