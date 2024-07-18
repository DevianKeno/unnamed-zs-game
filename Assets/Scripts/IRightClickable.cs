using System;
using UnityEngine;
using UZSG.Players;

namespace UZSG.Interactions
{
    /// <summary>
    /// Represents objects that the Player can perform secondary actions with (RMB by default).
    /// </summary>
    public interface IRightClickable
    {
        
        public abstract void RightClick(PlayerActions actor, InteractArgs args);
        public event EventHandler<InteractArgs> OnRightClick;
    }
}