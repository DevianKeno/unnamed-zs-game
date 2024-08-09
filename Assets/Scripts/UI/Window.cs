using System;

using UnityEngine;
using UnityEngine.UI;

using UZSG.Systems;

namespace UZSG.UI
{
    /// <summary>
    /// Windows are UI elements that can be shown or hidden.
    /// </summary>
    public class Window : MonoBehaviour, IUIElement
    {
        static Vector2 _hiddenWindowsPosition = new(5000, 5000);
        
        [SerializeField] protected RectTransform rect;
        public RectTransform Rect => rect;
        [field: Space]

        public bool IsVisible { get; set; }
        public Vector3 Position
        {
            get { return rect.transform.position; }
            set { rect.transform.position = value; }
        }
        public float FadeDuration = 0.3f;
        
        protected GameObject blocker;
        protected Vector2 showedPosition;


        #region Events

        /// <summary>
        /// Called whenever this Window is shown/opened.
        /// </summary>
        public event Action OnOpen;
        /// <summary>
        /// Called whenever this Window is hidden/closed.
        /// </summary>
        public event Action OnClose;

        #endregion
        

        void Awake()
        {
            rect = GetComponent<RectTransform>();
            showedPosition = rect.localPosition;
        }

        /// <summary>
        /// Executes once ONLY IF the window is previously hidden, then made visible.
        /// </summary>
        public virtual void OnShow() { }
        /// <summary>
        /// Executes once ONLY IF the window is previously visible, then made hidden.
        /// </summary>
        public virtual void OnHide() { }

        /// <summary>
        /// Shows the window.
        /// </summary>
        public void Show()
        {
            if (!IsVisible)
            {
                OnShow();
                OnOpen?.Invoke();
            }

            rect.anchoredPosition = showedPosition; /// hehe^2
            IsVisible = true;
        }

        /// <summary>
        /// Hides the window.
        /// Does not destroy nor disables the object.
        /// Use Destroy() if you need to delete, and SetActive() to disable.
        /// </summary>
        public void Hide()
        {
            if (IsVisible)
            {
                OnHide();
                OnClose?.Invoke();
            }

            rect.anchoredPosition = _hiddenWindowsPosition; /// hehe^2
            IsVisible = false;
        }

        public void SetActive(bool enabled)
        {
            if (enabled)
            {
                gameObject.SetActive(true);
                Show();
                LayoutRebuilder.ForceRebuildLayoutImmediate(transform as RectTransform);
            }
            else
            {
                Hide();
                // Game.UI.RemoveFromActiveWindows(this);
                gameObject.SetActive(false);
            }
        }

        // public void FadeIn()
        // {

        // }

        // public void FadeOut()
        // {

        // }

        public void Destroy(float delay = 0f)
        {
            Hide();
            Destroy(gameObject, delay);
        }

        public void ToggleVisibility()
        {
            SetVisible(!IsVisible);
        }

        public void SetVisible(bool visible)
        {
            if (visible)
            {
                Show();
            }
            else
            {
                Hide();
            }
            IsVisible = visible;
        }

        public void SetScale(float multiplier)
        {
            Vector2 dimensions = rect.localScale;
            dimensions.x *= multiplier; 
            dimensions.y *= multiplier; 
            rect.localScale = dimensions;
        }

        public void SetScale(float width, float height)
        {
            rect.localScale = new(width, height);
        }
        
        public void Move(Vector3 position)
        {
            rect.position = position;
        }
    }
}
