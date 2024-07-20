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

            Player.smMove.OnStateChanged += MovementStateChanged;
            Player.smAction.OnStateChanged += ActionStateChanged;
        }

        private void ActionStateChanged(object sender, StateMachine<ActionStates>.StateChangedContext e)
        {
            UI.actionStateText.text = e.To.ToString();
        }

        private void MovementStateChanged(object sender, StateMachine<MoveStates>.StateChangedContext e)
        {
            UI.movementStateText.text = e.To.ToString();
        }

        void OnDisable()
        {
            Player.OnDoneInit -= Init;
            Player.smMove.OnStateChanged -= MovementStateChanged;
            Player.smAction.OnStateChanged -= ActionStateChanged;
        }
    }
}