using System;

using UnityEngine;
using UnityEngine.UI;



namespace UZSG.UI
{
    /// <summary>
    /// Windows are UI elements that can be shown or hidden.
    /// </summary>
    public class Window : UIElement
    {
        public float FadeDuration = 0.3f;
        
        protected GameObject blocker;


        #region Events

        /// <summary>
        /// Called whenever this Window is shown/opened.
        /// </summary>
        public event Action OnOpened;
        /// <summary>
        /// Called whenever this Window is hidden/closed.
        /// </summary>
        public event Action OnClosed;

        #endregion
        
        
        /// <summary>
        /// Shows the window.
        /// </summary>
        public override void Show()
        {
            Game.UI.AddToActiveWindows(this);

            base.Show();
            OnOpened?.Invoke();
        }

        /// <summary>
        /// Hides the window.
        /// Does not destroy nor disables the object.
        /// Use Destroy() if you need to delete.
        /// </summary>
        public override void Hide()
        {
            Game.UI.RemoveFromActiveWindows(this);

            OnClosed?.Invoke();
            base.Hide();
        }
    }
}
