using System;

using UnityEngine;
using UnityEngine.UI;



namespace UZSG.UI
{
    /// <summary>
    /// Panels are UI elements that can be shown or hidden.
    /// </summary>
    public class Panel : UIElement
    {
        public float FadeDuration = 0.3f;
        
        protected GameObject blocker;


        #region Events

        /// <summary>
        /// Called whenever this Panel is shown/opened.
        /// </summary>
        public event Action OnOpened;
        /// <summary>
        /// Called whenever this Panel is hidden/closed.
        /// </summary>
        public event Action OnClosed;

        #endregion
        
        
        /// <summary>
        /// Shows the panel.
        /// </summary>
        public override void Show()
        {
            base.Show();
            OnOpened?.Invoke();
        }

        /// <summary>
        /// Hides the panel.
        /// Does not destroy nor disables the object.
        /// Use Destroy() if you need to delete.
        /// </summary>
        public override void Hide()
        {
            OnClosed?.Invoke();
            base.Hide();
        }
    }
}
