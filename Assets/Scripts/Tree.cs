using System;
using UnityEngine;
using UZSG.Interactions;
using UZSG.Items;
using UZSG.Players;

namespace UZSG
{
    public class Tree : ILeftClickable
    {
        public event EventHandler<InteractArgs> OnLeftClick;

        public void LeftClick(PlayerActions actor, InteractArgs args)
        {
            // if (actor.Player.Inventory,Equipped)
        }
    }
}
