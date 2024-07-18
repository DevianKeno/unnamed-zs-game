using System;
using UnityEngine;
using UZSG.Players;

namespace UZSG.Interactions
{
    /// <summary>
    /// Represents objects that the Player can perform primary actions with (LMB by default).
    /// </summary>
    public interface ILeftClickable
    {
        public abstract void LeftClick(PlayerActions actor, InteractArgs args);
        public event EventHandler<InteractArgs> OnLeftClick;
    }
}
