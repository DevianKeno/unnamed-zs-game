using System;
using TMPro;
using UnityEngine;
using UZSG.Entities;
using UZSG.Systems;
using UZSG.UI;

namespace UZSG.Players
{
    public class PlayerDebug : MonoBehaviour
    {
        public Player Player;
        public PlayerDebugUI UI;
        [SerializeField] GameObject UIPrefab;

        void Awake()
        {
            Player = GetComponent<Player>();
            Player.OnDoneInit += Init;
        }

        void Init(object sender, EventArgs e)
        {
            UI = Instantiate(UIPrefab, Game.UI.Canvas.transform).GetComponent<PlayerDebugUI>();

            Player.MoveStateMachine.OnStateChanged += MovementStateChanged;
            Player.ActionStateMachine.OnStateChanged += ActionStateChanged;
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

        void MovementStateChanged(object sender, StateMachine<MoveStates>.StateChangedContext e)
        {
            UI.movementStateText.text = $"Move state: {e.To}";
        }

        void ActionStateChanged(object sender, StateMachine<ActionStates>.StateChangedContext e)
        {
            UI.actionStateText.text = $"Action state: {e.To}";
        }

        void OnDisable()
        {
            Player.OnDoneInit -= Init;
            Player.MoveStateMachine.OnStateChanged -= MovementStateChanged;
            Player.ActionStateMachine.OnStateChanged -= ActionStateChanged;
        }
    }
}