using System;

using UnityEngine;
using UnityEngine.UI;

using UZSG.Systems;

namespace UZSG.UI
{
    /// <summary>
    /// Helper class for RectTransform pivot values.
    /// </summary>
    public class Pivot
    {
        public static Vector2 TopLeft => new (0, 1);
        public static Vector2 TopMiddle => new (0.5f, 1);
        public static Vector2 TopRight => new (1, 1);
        public static Vector2 MiddleLeft => new (0, 0.5f);
        public static Vector2 Center => new (0.5f, 0.5f);
        public static Vector2 MiddleRight => new (1, 0.5f);
        public static Vector2 BottomLeft => new (0, 0);
        public static Vector2 BottomMiddle => new (0.5f, 0);
        public static Vector2 BottomRight => new (1, 0);
    }

    /// <summary>
    /// Windows are UI elements that can be shown or hidden.
    /// </summary>
    public class Window : MonoBehaviour, IUIElement
    {
        [SerializeField] protected RectTransform rect;
        public RectTransform Rect => rect;
        [field: Space]

        public bool IsVisible { get; set; }
        public Vector2 Position
        {
            get { return rect.anchoredPosition; }
            set { rect.anchoredPosition = value; }
        }
        public Vector2 Pivot
        {
            get { return rect.pivot; }
            set { rect.pivot = value; }
        }
        public float FadeDuration = 0.3f;
        
        protected GameObject blocker;


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
            gameObject.SetActive(true);

            if (!IsVisible)
            {
                OnShow();
                OnOpen?.Invoke();
            }
            
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

            gameObject.SetActive(false);
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

        public void SetParent(Transform p)
        {
            transform.SetParent(p);
        }

        // public void FadeIn()
        // {

        // }

        // public void FadeOut()
        // {

        // }

        public void Destroy(float delay = 0f, bool invokeOnHideEvent = true)
        {
            if (invokeOnHideEvent) Hide();
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

        public void Rebuild()
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(rect);
        }
    }
}
