using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.InputSystem;

using UZSG.Systems;
using UZSG.Data;
using UZSG.Entities;
using UZSG.Players;
using UZSG.Items.Weapons;

namespace UZSG.Items.Tools
{
    public class HeldToolController : HeldItemController
    {
        public Player Player => owner as Player;
        public ToolData ToolData => ItemData as ToolData;

        public override void Initialize()
        {
            
        }

        public override void SetStateFromAction(ActionStates state)
        {
            
        }
    }
}